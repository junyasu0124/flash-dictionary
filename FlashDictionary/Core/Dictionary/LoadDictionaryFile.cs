using FlashDictionary.Core.Translation.InDictionaries;
using FlashDictionary.Util;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace FlashDictionary.Core.Dictionary;

internal enum DictionaryDataFormat
{
  Eijiro,
  TabSeparated,
}

internal enum LoadState
{
  Succeeded,
  FileNotExists,
  DataNotWritten,
  PositionsFileNotExists,
  UnknownErrored,
}

delegate bool TryMoveNext(out (string, List<Position>) current);

internal static class LoadDictionaryFile
{
  private const byte CRCharCode = 13; // \r
  private const byte LFCharCode = 10; // \n

  // insertCurrentDictionaryData  true: 今あるDictionaryDataの各単語の意味の末尾に読み込んだ意味を挿入する false: 今あるDictionaryDataファイルの末尾から、単語と意味のセットを付け加えていく
  // insertHead  true: 今ある意味よりも前に新しい意味を追加する、実際に表示される意味もその順番に
  [SuppressMessage("Style", "IDE0063")]
  public static async Task<LoadState> LoadAsync(string filePath, DictionaryDataFormat format, Encoding encoding, bool insertCurrentDictionaryData, bool insertHead)
  {
    if (File.Exists(filePath) == false)
      return LoadState.FileNotExists;

    try
    {
      var start = DateTime.Now;
      SortedDictionary<string, List<Position>>? items = await ReadAddingFile(filePath, format, encoding);
      Debug.WriteLine($"---{(DateTime.Now - start).TotalMilliseconds} ms---");
        
      if (items.Count == 0)
      {
        return LoadState.DataNotWritten;
      }

      // 新しいファイルを作る分のストレージの空きがあるかを確認

      var previousStreamPath = Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, Dictionary.DictionaryDataFileName);
      var newStreamPath = Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, Dictionary.DictionaryDataCacheFileName);

      var isPreviousStreamPathExists = Path.Exists(previousStreamPath);
      if (isPreviousStreamPathExists && File.Exists(Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, Dictionary.DictionaryPositionsFileName)) == false)
      {
        items = null;
        GC.Collect(2, GCCollectionMode.Aggressive, true);
        return LoadState.PositionsFileNotExists;
      }

      // 今あるDictionaryDataの各単語の意味の末尾に読み込んだ意味を挿入する
      if (insertCurrentDictionaryData)
      {
        SortedDictionary<TripleChar, Position?> positions = new(new TripleCharComparer());

        // 既存のDictionaryDataファイルが存在する
        if (isPreviousStreamPathExists)
        {
          using var previousStream = File.OpenRead(previousStreamPath);
          using var previousReader = new StreamReader(previousStream, Encoding.UTF8);

          var previousPositions = ReadPreviouses(previousStream);

          var merged = MergeOriginalAndPosition(previousPositions, items, insertHead);
          var mergedEnumerator = merged.GetEnumerator();
          await WriteToCacheAsync(previousStream, TryMoveNext);

          previousPositions.Clear();

          bool TryMoveNext(out (string, List<Position>) current)
          {
            if (mergedEnumerator.MoveNext())
            {
              current = mergedEnumerator.Current;
              return true;
            }
            else
            {
              current = default;
              return false;
            }
          }

          static SortedDictionary<string, Position> ReadPreviouses(FileStream previousStream)
          {
            const int bufferSize = 512;

            SortedDictionary<string, Position> result = new(new KeyComparer());

            previousStream.Seek(0, SeekOrigin.Begin);

            Span<byte> buffer = stackalloc byte[bufferSize];
            var originalStack = Array.Empty<byte>();
            var inDictionaryNameDeclaration = false;
            var originalTemp = string.Empty;
            var offsetTemp = 0;
            var originalOffset = -1;
            var i = 0;
            while (true)
            {
              var loadedSize = previousStream.Read(buffer);

              for (var j = 0; j < loadedSize; j++)
              {
                if (inDictionaryNameDeclaration)
                {
                  if (buffer[j] == (byte)Dictionary.DictionaryDataDictionaryNameDeclaration)
                  {
                    inDictionaryNameDeclaration = false;
                  }
                  continue;
                }

                switch (buffer[j])
                {
                  case (byte)Dictionary.DictionaryDataDictionaryNameDeclaration:
                    inDictionaryNameDeclaration = true;
                    break;
                  case (byte)Dictionary.DictionaryDataSeparatorBetweenWord:
                    result[originalTemp] = new(offsetTemp, bufferSize * i + j - offsetTemp);
                    originalOffset = j + 1;
                    break;
                  case (byte)Dictionary.DictionaryDataSeparatorBetweenOriginalAndMeaning:
                    if (originalOffset == -1)
                    {
                      originalTemp = Encoding.UTF8.GetString([.. originalStack, .. buffer[..j]]);
                      originalStack = [];
                    }
                    else
                    {
                      originalTemp = Encoding.UTF8.GetString(buffer[originalOffset..j]);
                      originalOffset = -1;
                    }
                    offsetTemp = bufferSize * i + j + 1;
                    break;
                }
              }

              if (originalOffset != -1)
              {
                originalStack = [.. originalStack, .. buffer[originalOffset..]];
                originalOffset = -1;
              }

              if (loadedSize != bufferSize)
              {
                result[originalTemp] = new(offsetTemp, bufferSize * i + loadedSize - offsetTemp);
                break;
              }

              i++;
            }
            return result;
          }

          static IEnumerable<(string, List<Position>)> MergeOriginalAndPosition(SortedDictionary<string, Position> previousItems, SortedDictionary<string, List<Position>> addedItems, bool insertHead)
          {
            var previousEnumerator = previousItems.GetEnumerator();
            var addedEnumerator = addedItems.GetEnumerator();
            previousEnumerator.MoveNext();
            addedEnumerator.MoveNext();

            while (true)
            {
              if (previousEnumerator.Current.Key == addedEnumerator.Current.Key)
              {
                if (insertHead)
                  yield return (previousEnumerator.Current.Key, [.. addedEnumerator.Current.Value, new(0 - previousEnumerator.Current.Value.Offset, previousEnumerator.Current.Value.Length)]);
                else
                  yield return (previousEnumerator.Current.Key, [new(0 - previousEnumerator.Current.Value.Offset, previousEnumerator.Current.Value.Length), .. addedEnumerator.Current.Value]);
                if (previousEnumerator.MoveNext() == false)
                {
                  do
                  {
                    yield return (addedEnumerator.Current.Key, addedEnumerator.Current.Value);
                  } while (addedEnumerator.MoveNext());
                  yield break;
                }
                if (addedEnumerator.MoveNext() == false)
                {
                  yield return (previousEnumerator.Current.Key, [new(0 - previousEnumerator.Current.Value.Offset, previousEnumerator.Current.Value.Length)]);
                  do
                  {
                    yield return (previousEnumerator.Current.Key, [new(0 - previousEnumerator.Current.Value.Offset, previousEnumerator.Current.Value.Length)]);
                  } while (previousEnumerator.MoveNext());
                  yield break;
                }
              }
              else if (KeyComparer.StaticCompare(previousEnumerator.Current.Key, addedEnumerator.Current.Key) < 0)
              {
                yield return (previousEnumerator.Current.Key, [new(0 - previousEnumerator.Current.Value.Offset, previousEnumerator.Current.Value.Length)]);
                if (previousEnumerator.MoveNext() == false)
                {
                  do
                  {
                    yield return (addedEnumerator.Current.Key, addedEnumerator.Current.Value);
                  } while (addedEnumerator.MoveNext());
                  yield break;
                }
              }
              else
              {
                yield return (addedEnumerator.Current.Key, addedEnumerator.Current.Value);
                if (addedEnumerator.MoveNext() == false)
                {
                  do
                  {
                    yield return (previousEnumerator.Current.Key, [new(0 - previousEnumerator.Current.Value.Offset, previousEnumerator.Current.Value.Length)]);
                  } while (previousEnumerator.MoveNext());
                  yield break;
                }
              }
            }
          }
        }
        // 既存のDictionaryDataファイルが存在しない
        else
        {
          var enumerator = items.GetEnumerator();
          await WriteToCacheAsync(null, TryMoveNext);

          bool TryMoveNext(out (string, List<Position>) current)
          {
            if (enumerator.MoveNext())
            {
              current = (enumerator.Current.Key, enumerator.Current.Value);
              return true;
            }
            else
            {
              current = default;
              return false;
            }
          }
        }

        foreach (var key in EnumerateCharPairs())
        {
          if (positions.TryGetValue(key, out var value) == false)
          {
            value = null;
            positions[key] = value;
          }
        }

        WritePositionsFile(positions.Select(x => (x.Key, x.Value)));

        Dictionary.SetPositions(positions.ToDictionary(x => x.Key, y => PositionItemToArray(y.Value)));

        await RenameDictionaryDataCacheFileAsync();

        items = null;
        GC.Collect(2, GCCollectionMode.Aggressive, true);
        return LoadState.Succeeded;

        async Task WriteToCacheAsync(FileStream? previousStream, TryMoveNext tryMoveNext)
        {
          using (var newStream = File.Create(newStreamPath))
          {
            long currentOffset = 0;
            long offsetTemp = 0;
            TripleChar? previousKey = null;

            var isNeedToEncodingConvert = encoding != Encoding.UTF8;

            tryMoveNext(out var current);

            using (var inputStream = File.OpenRead(filePath))
            {
              while (true)
              {
                var currentOffsetTemp = currentOffset;

                var (original, positionList) = current;
                var currentKey = GetValueInDictionaries.ConvertKeyToKeyInPositions(original);
                previousKey ??= currentKey;

                var originalBytes = Encoding.UTF8.GetBytes(original);
                newStream.Write(originalBytes);
                newStream.WriteByte((byte)Dictionary.DictionaryDataSeparatorBetweenOriginalAndMeaning);
                currentOffset += originalBytes.Length + 1;

                for (var i = 0; i < positionList.Count; i++)
                {
                  var (offset, length) = positionList[i];

                  byte[] bytes = new byte[length];
                  // previous
                  if (offset < 0)
                  {
                    previousStream!.Seek(0 - offset, SeekOrigin.Begin);
                    await previousStream.ReadAsync(bytes.AsMemory());

                    await newStream.WriteAsync(bytes);
                  }
                  // added
                  else
                  {
                    inputStream.Seek(offset, SeekOrigin.Begin);
                    await inputStream.ReadAsync(bytes.AsMemory());

                    if (isNeedToEncodingConvert)
                    {
                      var convertedBytes = Encoding.Convert(encoding, Encoding.UTF8, bytes);
                      await newStream.WriteAsync(convertedBytes);
                      length = convertedBytes.Length;
                    }
                    else
                    {
                      await newStream.WriteAsync(bytes);
                    }
                  }

                  if (i != positionList.Count - 1)
                  {
                    newStream.WriteByte((byte)Dictionary.DictionaryDataSeparatorBetweenMeanings);
                    currentOffset += length + 1;
                  }
                  else
                  {
                    currentOffset += length;
                  }
                }

                if (tryMoveNext(out current))
                {
                  newStream.WriteByte((byte)Dictionary.DictionaryDataSeparatorBetweenWord);
                  currentOffset++;
                  if (currentKey != previousKey.Value)
                  {
                    try
                    {
                      positions.Add(previousKey.Value, new(offsetTemp, currentOffset - offsetTemp));
                      previousKey = currentKey;
                      offsetTemp = currentOffsetTemp;
                    }
                    catch (Exception e)
                    {

                    }
                  }
                }
                else
                {
                  if (previousKey.Value != currentKey)
                    positions.Add(previousKey.Value, new(offsetTemp, currentOffsetTemp - offsetTemp));

                  positions.Add(currentKey, new(currentOffsetTemp, currentOffset - offsetTemp));
                  break;
                }
              }
            }
          }
        }
      }
      // 今あるDictionaryDataファイルの末尾（先頭）から、単語と意味のセットを付け加えていく
      else
      {
        // 既存のDictionaryDataファイルが存在する
        if (isPreviousStreamPathExists)
        {
          var previousPositions = await ReadPreviousPositionsFileAsync();

          long previousPositionsOffset = 0;
          long addedPositionsOffset = 0;

          long offsetOffset = 0;
          ((TripleChar key, long offset)[] offsets, long fileLength)? writeAddedResult = null;

          if (insertHead)
          {
            using (var newStream = File.Create(newStreamPath))
            {
              writeAddedResult = await WriteAddedAsync(newStream);
            }
          }

          using (var newStream = insertHead ? File.Open(newStreamPath, FileMode.Append) : File.Create(newStreamPath))
          {
            using var previousStream = File.OpenRead(previousStreamPath);
            using var previousReader = new StreamReader(previousStream, Encoding.UTF8);

            if (insertHead)
            {
              previousPositionsOffset = newStream.Length;
              offsetOffset = newStream.Length;
            }

            byte[] buffer = new byte[1];
            await previousStream.ReadAsync(buffer.AsMemory(0, 1));
            if (Convert.ToChar(buffer[0]) != Dictionary.DictionaryDataDictionaryNameDeclaration)
            {
              var dictionaryNameBytes = Encoding.UTF8.GetBytes(""); // 適当な辞書の名前

              newStream.WriteByte((byte)Dictionary.DictionaryDataDictionaryNameDeclaration);
              await newStream.WriteAsync(dictionaryNameBytes);
              newStream.WriteByte((byte)Dictionary.DictionaryDataDictionaryNameDeclaration);
            }
            previousStream.Seek(0, SeekOrigin.Begin);
            await previousStream.CopyToAsync(newStream);
          }

          if (insertHead == false)
          {
            using var newStream = File.Open(newStreamPath, FileMode.Append);
            addedPositionsOffset = newStream.Length;
            writeAddedResult = await WriteAddedAsync(newStream);
          }
          var writeAddedPositions = OrganizeWriteAddedAsyncRetuned(writeAddedResult!.Value);
          var positions = MergePositions(writeAddedPositions, previousPositions, addedPositionsOffset, previousPositionsOffset, insertHead).ToArray();
          WritePositionsFile(positions);

          Dictionary.SetPositions(positions.ToDictionary(x => x.key, y => y.positions));
        }
        // 既存のDictionaryDataファイルが存在しない
        else
        {
          ((TripleChar key, long offset)[] offsets, long fileLength) writeAddedResult;
          using (var newStream = File.OpenWrite(newStreamPath))
          {
            writeAddedResult = await WriteAddedAsync(newStream);
          }
          var writeAddedPositions = OrganizeWriteAddedAsyncRetuned(writeAddedResult);

          var included = IncludeAllKeys(writeAddedPositions).ToArray();
          WritePositionsFile(included);

          Dictionary.SetPositions(included.ToDictionary(x => x.key, y => PositionItemToArray(y.position)));
        }

        await RenameDictionaryDataCacheFileAsync();

        items = null;
        GC.Collect(2, GCCollectionMode.Aggressive, true);
        return LoadState.Succeeded;

        async Task<((TripleChar key, long offset)[] offsets, long fileLength)> WriteAddedAsync(FileStream newStream)
        {
          var dictionaryNameBytes = Encoding.UTF8.GetBytes(""); // 任意の辞書の名前

          newStream.WriteByte((byte)Dictionary.DictionaryDataDictionaryNameDeclaration);
          await newStream.WriteAsync(dictionaryNameBytes);
          newStream.WriteByte((byte)Dictionary.DictionaryDataDictionaryNameDeclaration);

          var positions = new (TripleChar key, long offset)[items.Count];
          long offsetTemp = 2 + dictionaryNameBytes.Length;

          var isNeedToEncodingConvert = encoding != Encoding.UTF8;

          var iterator = items.GetEnumerator();
          iterator.MoveNext();
          using (var inputStream = File.OpenRead(filePath))
          {
            for (var i = 0; i < items.Count; i++)
            {
              var (original, positionList) = iterator.Current;
              positions[i].key = GetValueInDictionaries.ConvertKeyToKeyInPositions(original);
              positions[i].offset = offsetTemp;

              var originalBytes = Encoding.UTF8.GetBytes(original);
              await newStream.WriteAsync(originalBytes);
              newStream.WriteByte((byte)Dictionary.DictionaryDataSeparatorBetweenOriginalAndMeaning);

              offsetTemp += originalBytes.Length + 1;
              for (var j = 0; j < positionList.Count; j++)
              {
                var (offset, length) = positionList[j];

                inputStream.Seek(offset, SeekOrigin.Begin);
                byte[] bytes = new byte[length];
                await inputStream.ReadAsync(bytes.AsMemory());

                if (isNeedToEncodingConvert)
                {
                  var convertedBytes = Encoding.Convert(encoding, Encoding.UTF8, bytes);
                  await newStream.WriteAsync(convertedBytes);

                  offsetTemp += convertedBytes.Length + 1;
                }
                else
                {
                  await newStream.WriteAsync(bytes);

                  offsetTemp += length + 1;
                }

                if (j != positionList.Count - 1)
                  newStream.WriteByte((byte)Dictionary.DictionaryDataSeparatorBetweenMeanings);
              }

              if (iterator.MoveNext())
                newStream.WriteByte((byte)Dictionary.DictionaryDataSeparatorBetweenWord);
              else
                return (positions, offsetTemp);
            }
          }
          throw new Exception("items.Count must be greater than 1.");
        }
        IEnumerable<(TripleChar key, Position? position)> OrganizeWriteAddedAsyncRetuned(((TripleChar key, long offset)[] offsets, long fileLegnth) returned)
        {
          List<(TripleChar key, Position position)> result = [];
          var items = returned.offsets.GroupBy(x => x.key, new TripleCharEqualityComparer()).OrderBy(x => x.Key, new TripleCharComparer()).Select(x => (key: x.Key, offset: x.Min(y => y.offset))).ToArray();

          for (var i = 0; i < items.Length - 1; i++)
          {
            yield return (items[i].key, new(items[i].offset, items[i + 1].offset - items[i].offset));
          }
          yield return (items[^1].key, new(items[^1].offset, returned.fileLegnth - items[^1].offset));
        }
        IEnumerable<(TripleChar key, Position? position)> IncludeAllKeys(IEnumerable<(TripleChar key, Position? position)> data)
        {
          var enumerator = data.GetEnumerator();
          enumerator.MoveNext();
          foreach (var key in EnumerateCharPairs())
          {
            if (enumerator.Current.key == key)
            {
              yield return enumerator.Current;
              enumerator.MoveNext();
            }
            else
            {
              yield return (key, null);
            }
          }
        }
      }
    }
    catch (Exception e)
    {
      return LoadState.UnknownErrored;
    }
  }

  private const string dataFormatWordKey = "{WORD}";
  private const string dataFormatMeaningKey = "{MEANING}";
  private static readonly SortedList<DictionaryDataFormat, string> dictionaryDataFormat = new()
  {
    [DictionaryDataFormat.Eijiro] = $"■{dataFormatWordKey} : {dataFormatMeaningKey}",
    [DictionaryDataFormat.TabSeparated] = $"{dataFormatWordKey}\t{dataFormatMeaningKey}",
  };
  private static string? separatorBetweenOriginalAndMeaning = null;
  private static string? prefix = null;
  private static string? suffix = null;
  private readonly static char[] leftBrackets = ['<', '[', '{', '＜', '［', '｛', '｟', '｢', '〈', '《', '「', '『', '【', '〔', '〖', '〘', '〚', '⟦', '⟨', '⟪', '⟬', '⟮', '⦃', '⦅', '⦇', '⦉', '⦋', '⦍', '⦏', '⦑', '⦗', '⧼', '❨', '❪', '❬', '❮', '❰', '❲', '❴', '⁽', '₍'];

  private async static Task<SortedDictionary<string, List<Position>>> ReadAddingFile(string filePath, DictionaryDataFormat format, Encoding encoding)
  {
    SortedDictionary<string, List<Position>> items = new(new KeyComparer());
    using (var inputStream = File.OpenRead(filePath))
    {
      using var inputReader = new StreamReader(inputStream, encoding);
      string? line = await inputReader.ReadLineAsync();

      if (line != null)
      {
        long lineBreakLength = 0;
        var positionAfterReadOnce = inputReader.BaseStream.Position;

        var firstLineByteCount = encoding.GetByteCount(line);
        if (inputReader.BaseStream.Length > firstLineByteCount)
        {
          inputReader.BaseStream.Seek(firstLineByteCount, SeekOrigin.Begin);
          byte[] buffer = new byte[2];
          await inputReader.BaseStream.ReadAsync(buffer.AsMemory(0, 2));
          inputReader.BaseStream.Seek(positionAfterReadOnce, SeekOrigin.Begin);

          if (buffer[0] == CRCharCode && buffer[1] == LFCharCode)
            lineBreakLength = 2;
          else if (buffer[0] == CRCharCode || buffer[0] == LFCharCode)
            lineBreakLength = 1;
        }

        long position = 0;
        do
        {
          var parsed = ParseLine(line, format, encoding);

          foreach (var item in parsed)
          {
            if (item.HasValue)
            {
              if (items.TryGetValue(item.Value.original, out var value) == false)
              {
                value = ([]);
                items[item.Value.original] = value;
              }
              value.Add(new(item.Value.meaningOffset + position, item.Value.meaningLength));
            }
          }

          position += encoding.GetByteCount(line) + lineBreakLength;
        } while ((line = await inputReader.ReadLineAsync()) != null);
      }
    }

    return items;
  }

  private static IEnumerable<(string original, long meaningOffset, long meaningLength)?> ParseLine(string line, DictionaryDataFormat format, Encoding encoding)
  {
    if (string.IsNullOrWhiteSpace(line))
      yield break;

    separatorBetweenOriginalAndMeaning ??= dictionaryDataFormat[format].Split(dataFormatWordKey)[1].Split(dataFormatMeaningKey)[0];
    prefix ??= dictionaryDataFormat[format].Split(dataFormatWordKey)[0];
    suffix ??= dictionaryDataFormat[format].Split(dataFormatMeaningKey)[1];

    var separatedLine = line.Split(separatorBetweenOriginalAndMeaning);

    if (separatedLine.Length != 2 || separatedLine[0].Length < prefix.Length || separatedLine[1].Length < suffix.Length || separatedLine[0].StartsWith(prefix, StringComparison.Ordinal) == false || separatedLine[1].EndsWith(suffix, StringComparison.Ordinal) == false)
    {
      yield return null;
      yield break;
    }

    var originalWord = separatedLine[0][prefix.Length..];

    var removedParentheses = originalWord.Replace("(", "").Replace(")", "");
    IEnumerable<string> words = originalWord.Length == removedParentheses.Length ? [originalWord] : [originalWord, removedParentheses];
    foreach (var word in words)
    {
      var bracketIndex = word.IndexOfAny(leftBrackets);

      string original;
      string targetString;
      if (bracketIndex == -1)
      {
        original = word;
        targetString = separatedLine[1][..^suffix.Length].Trim();
      }
      else
      {
        original = word[..bracketIndex].Trim();
        targetString = line[(bracketIndex + prefix.Length)..^suffix.Length].Trim();
      }

      var targetIndex = line.IndexOf(targetString);
      if (targetIndex != -1)
      {
        long meaningOffset = encoding.GetByteCount(line[..targetIndex]);
        long meaningLength = encoding.GetByteCount(line[targetIndex..(targetIndex + targetString.Length)]);
        yield return (original, meaningOffset, meaningLength);
      }
    }
  }

  private static async Task<SortedDictionary<TripleChar, Position[]>> ReadPreviousPositionsFileAsync()
  {
    string fileContent;
    using (var stream = File.OpenRead(Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, Dictionary.DictionaryPositionsFileName)))
    {
      using var reader = new StreamReader(stream, Encoding.UTF8);

      fileContent = await reader.ReadToEndAsync();
    }

    SortedDictionary<TripleChar, Position[]> result = new(new TripleCharComparer());

    foreach (var keyGroup in fileContent.Trim().Split(Dictionary.DictionaryPositionsSeparatorBetweenKey))
    {
      if (keyGroup == null)
        continue;

      ProcessItem(keyGroup, result);
    }

    static void ProcessItem(string keyGroup, SortedDictionary<TripleChar, Position[]> result)
    {
      var success = Split.TrySplitIntoTwo(keyGroup, Dictionary.DictionaryPositionsSeparatorBetweenKeyAndOther, out ReadOnlySpan<char> first, out string second);

      if (success && first.IsEmpty == false)
      {
        var dicts = second.Split(Dictionary.DictionaryPositionsSeparatorBetweenDictionary);
        var positions = new Position[dicts.Length];
        for (var i = 0; i < dicts.Length; i++)
        {
          var offsetAndLength = dicts[i].Split(Dictionary.DictionaryPositionSeparatorBetweenOffsetAndLength);
          if (offsetAndLength.Length == 2)
          {
            positions[i] = new(long.Parse(offsetAndLength[0]), long.Parse(offsetAndLength[1]));
            if (i == dicts.Length - 1)
            {
              result[GetValueInDictionaries.ConvertKeyToKeyInPositions(first)] = positions;
            }
          }
          else
          {
            break;
          }
        }
      }
    }

    return result;
  }

  private static IEnumerable<(TripleChar key, Position[]? positions)> MergePositions(IEnumerable<(TripleChar key, Position? position)> organizedAddedPositions, SortedDictionary<TripleChar, Position[]> previousPositions, long addedPositionsOffset, long previousPositionsOffset, bool insertHead)
  {
    var addedEnumerator = organizedAddedPositions.GetEnumerator();
    var doNext = false;
    foreach (var key in EnumerateCharPairs())
    {
      if (doNext || addedEnumerator.MoveNext())
      {
        doNext = false;
        if (addedEnumerator.Current.key == key && addedEnumerator.Current.position.HasValue)
        {
          if (previousPositions.TryGetValue(key, out var value))
          {
            if (insertHead)
              yield return (key, previousPositionsOffset == 0 ?
                [new(addedEnumerator.Current.position.Value.Offset + addedPositionsOffset, addedEnumerator.Current.position.Value.Length), .. value] :
                [new(addedEnumerator.Current.position.Value.Offset, addedEnumerator.Current.position.Value.Length), .. value.Select(x => new Position(x.Offset + previousPositionsOffset, x.Length))]);
            else
              yield return (key, previousPositionsOffset == 0 ?
                [.. value, new(addedEnumerator.Current.position.Value.Offset + addedPositionsOffset, addedEnumerator.Current.position.Value.Length)] :
                [.. value.Select(x => new Position(x.Offset + previousPositionsOffset, x.Length)), new(addedEnumerator.Current.position.Value.Offset, addedEnumerator.Current.position.Value.Length)]);
          }
          else
          {
            yield return (key, [new(addedEnumerator.Current.position.Value.Offset + addedPositionsOffset, addedEnumerator.Current.position.Value.Length)]);
          }
        }
        else
        {
          if (previousPositions.TryGetValue(key, out var value))
            yield return (key, previousPositionsOffset == 0 ? value : [.. value.Select(x => new Position(x.Offset + previousPositionsOffset, x.Length))]);
          else
            yield return (key, null);
          doNext = true;
        }
      }
      else
      {
        if (previousPositions.TryGetValue(key, out var value))
          yield return (key, previousPositionsOffset == 0 ? value : [.. value.Select(x => new Position(x.Offset + previousPositionsOffset, x.Length))]);
        else
          yield return (key, null);
      }
    }
  }

  private static void WritePositionsFile((TripleChar key, Position[]? positions)[] positionsList)
  {
    using var positionsFile = File.Create(Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, Dictionary.DictionaryPositionsFileName));
    using var writer = new StreamWriter(positionsFile, Encoding.UTF8);

    for (var i = 0; i < positionsList.Length; i++)
    {
      var (key, positions) = positionsList[i];

      writer.Write(key.ToString());
      writer.Write(Dictionary.DictionaryPositionsSeparatorBetweenKeyAndOther);

      if (positions == null || positions.Length == 0)
        writer.Write(Dictionary.DictionaryPositionsNull);
      else
      {
        for (var j = 0; j < positions.Length; j++)
        {
          writer.Write(positions[j].Offset);
          writer.Write(Dictionary.DictionaryPositionSeparatorBetweenOffsetAndLength);
          writer.Write(positions[j].Length);
          if (j != positions.Length - 1)
            writer.Write(Dictionary.DictionaryPositionsSeparatorBetweenDictionary);
        }
      }

      if (i != positionsList.Length - 1)
        writer.Write(Dictionary.DictionaryPositionsSeparatorBetweenKey);
      else
        break;
    }
  }
  private static void WritePositionsFile(IEnumerable<(TripleChar key, Position? positions)> positionsList)
  {
    using var positionsFile = File.Create(Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, Dictionary.DictionaryPositionsFileName));
    using var writer = new StreamWriter(positionsFile, Encoding.UTF8);

    var enumerator = positionsList.GetEnumerator();
    enumerator.MoveNext();
    while (true)
    {
      var (key, positions) = enumerator.Current;

      writer.Write(key.ToString());
      writer.Write(Dictionary.DictionaryPositionsSeparatorBetweenKeyAndOther);

      if (positions == null)
        writer.Write(Dictionary.DictionaryPositionsNull);
      else
      {
        writer.Write(positions.Value.Offset);
        writer.Write(Dictionary.DictionaryPositionSeparatorBetweenOffsetAndLength);
        writer.Write(positions.Value.Length);
      }

      if (enumerator.MoveNext())
        writer.Write(Dictionary.DictionaryPositionsSeparatorBetweenKey);
      else
        break;
    }
  }

  private static Position[]? PositionItemToArray(Position? position)
  {
    if (position.HasValue)
      return [position.Value];
    else
      return null;
  }

  private async static Task RenameDictionaryDataCacheFileAsync()
  {
    var cacheFile = await ApplicationData.Current.LocalCacheFolder.GetFileAsync(Dictionary.DictionaryDataCacheFileName);
    await cacheFile.RenameAsync(Dictionary.DictionaryDataFileName, NameCollisionOption.ReplaceExisting);
  }

  public static IEnumerable<TripleChar> EnumerateCharPairs()
  {
    yield return new('#');
    for (char i = 'a'; i <= 'z'; i++)
    {
      yield return new TripleChar(i);
      yield return new TripleChar(i, '#');

      for (char j = 'a'; j <= 'z'; j++)
      {
        yield return new TripleChar(i, j);
        yield return new TripleChar(i, j, '#');
        for (char k = 'a'; k <= 'z'; k++)
        {
          yield return new TripleChar(i, j, k);
        }
      }
    }
  }
}
