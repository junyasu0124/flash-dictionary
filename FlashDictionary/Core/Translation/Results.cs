using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.ViewManagement;

namespace FlashDictionary.Core.Translation;

internal class Results
{
  public Results()
  {
    Items = [];
  }
  public Results(List<(string Original, string[] Meaning)> items)
  {
    Items = items.Select(item => new Result(item)).ToList();
  }

  public List<Result> Items { get; }

  public void AddItems(List<(string Original, string[] Meaning)> items)
  {
    Items.AddRange(items.Select(item => new Result(item)));
  }
}

internal class Result((string Word, string[] Meanings) item)
{
  public string Word { get; } = item.Word;
  public string[] Meanings { get; } = item.Meanings;

  public RichTextBlock GenerateTextBlock()
  {
    var textBlock = new RichTextBlock
    {
      Blocks =
      {
        GenerateWordParagraph()
      }
    };
    var meaningsParagraph = GenerateMeaningsParagraph();
    for (var i = 0; i < meaningsParagraph.Count; i++)
    {
      textBlock.Blocks.Add(meaningsParagraph[i]);
    }
    return textBlock;
  }
  private Paragraph GenerateWordParagraph()
  {
    return new Paragraph
    {
      Inlines =
      {
        new Run
        {
          Text = Word,
        },
      }
    };

  }
  private List<Paragraph> GenerateMeaningsParagraph()
  {
    var foregroundInBracket = new SolidColorBrush(new UISettings().GetColorValue(UIColorType.AccentLight1));

    List<Paragraph> paragraphs = [];

    foreach (var meaning in Meanings)
    {
      List<Range> bracketPairs = [];

      // true; left brackets, false: right brackets
      bool searchingMode = true;
      int leftBracketIndex = -1;
      int searchingRightBracketIndex = -1;
      for (var i = 0; i < meaning.Length; i++)
      {
        if (searchingMode)
        {
          var index = meaning.IndexOfAny(leftBrackets, i);
          if (index == -1)
          {
            break;
          }
          leftBracketIndex = index;
          searchingMode = false;
          searchingRightBracketIndex = Array.IndexOf(leftBrackets, meaning.AsSpan()[index]);
          i = index;
        }
        else
        {
          var index = meaning.IndexOf(rightBrackets[searchingRightBracketIndex], i);
          if (index == -1)
          {
            break;
          }
          bracketPairs.Add(new Range(leftBracketIndex, index));
          searchingMode = true;
          i = index;
        }
      }

      var paragraph = new Paragraph();

      int previousEnd = 0;
      for (var i = 0; i < bracketPairs.Count; i++)
      {
        var range = bracketPairs[i];
        if (previousEnd != range.Start.Value)
        {
          paragraph.Inlines.Add(new Run
          {
            Text = meaning[previousEnd..range.Start.Value],
          });
        }
        paragraph.Inlines.Add(new Run
        {
          Text = meaning[range.Start.Value..(range.End.Value + 1)],
          Foreground = foregroundInBracket,
        });
        previousEnd = range.End.Value + 1;
      }
      if (previousEnd != meaning.Length)
      {
        paragraph.Inlines.Add(new Run
        {
          Text = meaning[previousEnd..],
        });
      }

      paragraphs.Add(paragraph);
    }

    return paragraphs;
  }
  private readonly static char[] leftBrackets = ['(', '（', '<', '[', '{', '＜', '［', '｛', '｟', '｢', '〈', '《', '【', '〔', '〖', '〘', '〚', '⟦', '⟨', '⟪', '⟬', '⟮', '⦃', '⦅', '⦇', '⦉', '⦋', '⦍', '⦏', '⦑', '⦗', '⧼', '❨', '❪', '❬', '❮', '❰', '❲', '❴', '⁽', '₍'];
  private readonly static char[] rightBrackets = [')', '）', '>', ']', '}', '＞', '］', '｝', '｠', '｣', '〉', '》', '】', '〕', '〗', '〙', '〛', '⟧', '⟩', '⟫', '⟭', '⟯', '⦄', '⦆', '⦈', '⦊', '⦌', '⦎', '⦐', '⦒', '⦘', '⧽', '❩', '❫', '❭', '❯', '❱', '❳', '❵', '⁾', '₎'];
}
