using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FlashDictionary.Core.Translation.InDictionaries.Normalization;

internal static partial class Paraphrase
{
  /// <summary>
  /// Replaces prepositions with a placeholder in a sentence.
  /// </summary>
  /// <param name="sentence">A sentence to normalize.</param>
  /// <returns>An array of original sentence and normalized sentences.</returns>
  public static IEnumerable<string> Normalize(string sentence)
  {
    yield return sentence;

    var words = sentence.Split(' ');
    if (words.Length < 2)
    {
      yield break;
    }
    foreach (var (word, function) in regexes)
    {
      var newSentence = function.Replace(sentence, $"$1 a {word} b");
      if (newSentence != sentence)
      {
        yield return newSentence;
      }
      newSentence = function.Replace(sentence, $"$1 a {word}");
      if (newSentence != sentence)
      {
        yield return newSentence;
      }
      newSentence = function.Replace(sentence, $"$1 {word} a");
      if (newSentence != sentence)
      {
        yield return newSentence;
      }

      newSentence = function.Replace(sentence, $"$1 ~ {word} ~");
      if (newSentence != sentence)
      {
        yield return newSentence;
      }
      newSentence = function.Replace(sentence, $"$1 ~ {word}");
      if (newSentence != sentence)
      {
        yield return newSentence;
      }
      newSentence = function.Replace(sentence, $"$1 {word} ~");
      if (newSentence != sentence)
      {
        yield return newSentence;
      }
    }
  }

  // ["about", "above", "across", "after", "against", "ahead", "along", "amid", "among", "apart", "around", "aside", "at", "away", "back", "before", "behind", "below", "beside", "between", "beyond", "by", "down", "downward", "during", "except", "for", "forward", "from", "in", "inside", "into", "like", "near", "of", "off", "on", "onto", "out", "outside", "over", "over to", "past", "round", "thought", "through", "throughout", "to", "together", "toward", "towards", "under", "underneath", "until", "up", "upon", "via", "with", "within", "without"];

  private static readonly IEnumerable<(string word, Regex function)> regexes = [
    ("about", adverbialsAbout()),
    ("above", adverbialsAbove()),
    ("across", adverbialsAcross()),
    ("after", adverbialsAfter()),
    ("against", adverbialsAgainst()),
    ("ahead", adverbialsAhead()),
    ("along", adverbialsAlong()),
    ("amid", adverbialsAmid()),
    ("among", adverbialsAmong()),
    ("apart", adverbialsApart()),
    ("around", adverbialsAround()),
    ("aside", adverbialsAside()),
    ("at", adverbialsAt()),
    ("away", adverbialsAway()),
    ("back", adverbialsBack()),
    ("before", adverbialsBefore()),
    ("behind", adverbialsBehind()),
    ("below", adverbialsBelow()),
    ("beside", adverbialsBeside()),
    ("between", adverbialsBetween()),
    ("beyond", adverbialsBeyond()),
    ("by", adverbialsBy()),
    ("down", adverbialsDown()),
    ("downward", adverbialsDownward()),
    ("during", adverbialsDuring()),
    ("except", adverbialsExcept()),
    ("for", adverbialsFor()),
    ("forward", adverbialsForward()),
    ("from", adverbialsFrom()),
    ("in", adverbialsIn()),
    ("inside", adverbialsInside()),
    ("into", adverbialsInto()),
    ("like", adverbialsLike()),
    ("near", adverbialsNear()),
    ("of", adverbialsOf()),
    ("off", adverbialsOff()),
    ("on", adverbialsOn()),
    ("onto", adverbialsOnto()),
    ("out", adverbialsOut()),
    ("outside", adverbialsOutside()),
    ("over", adverbialsOver()),
    ("past", adverbialsPast()),
    ("round", adverbialsRound()),
    ("thought", adverbialsThought()),
    ("through", adverbialsThrough()),
    ("throughout", adverbialsThroughout()),
    ("to", adverbialsTo()),
    ("together", adverbialsTogether()),
    ("toward", adverbialsToward()),
    ("towards", adverbialsTowards()),
    ("under", adverbialsUnder()),
    ("underneath", adverbialsUnderneath()),
    ("until", adverbialsUntil()),
    ("up", adverbialsUp()),
    ("upon", adverbialsUpon()),
    ("via", adverbialsVia()),
    ("with", adverbialsWith()),
    ("within", adverbialsWithin()),
    ("without", adverbialsWithout())
  ];

  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} about$")]
  private static partial Regex adverbialsAbout();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} above$")]
  private static partial Regex adverbialsAbove();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} across$")]
  private static partial Regex adverbialsAcross();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} after$")]
  private static partial Regex adverbialsAfter();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} against$")]
  private static partial Regex adverbialsAgainst();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} ahead$")]
  private static partial Regex adverbialsAhead();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} along$")]
  private static partial Regex adverbialsAlong();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} amid$")]
  private static partial Regex adverbialsAmid();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} among$")]
  private static partial Regex adverbialsAmong();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} apart$")]
  private static partial Regex adverbialsApart();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} around$")]
  private static partial Regex adverbialsAround();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} aside$")]
  private static partial Regex adverbialsAside();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} at$")]
  private static partial Regex adverbialsAt();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} away$")]
  private static partial Regex adverbialsAway();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} back$")]
  private static partial Regex adverbialsBack();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} before$")]
  private static partial Regex adverbialsBefore();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} behind$")]
  private static partial Regex adverbialsBehind();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} below$")]
  private static partial Regex adverbialsBelow();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} beside$")]
  private static partial Regex adverbialsBeside();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} between$")]
  private static partial Regex adverbialsBetween();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} beyond$")]
  private static partial Regex adverbialsBeyond();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} by$")]
  private static partial Regex adverbialsBy();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} down$")]
  private static partial Regex adverbialsDown();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} downward$")]
  private static partial Regex adverbialsDownward();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} during$")]
  private static partial Regex adverbialsDuring();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} except$")]
  private static partial Regex adverbialsExcept();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} for$")]
  private static partial Regex adverbialsFor();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} forward$")]
  private static partial Regex adverbialsForward();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} from$")]
  private static partial Regex adverbialsFrom();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} in$")]
  private static partial Regex adverbialsIn();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} inside$")]
  private static partial Regex adverbialsInside();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} into$")]
  private static partial Regex adverbialsInto();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} like$")]
  private static partial Regex adverbialsLike();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} near$")]
  private static partial Regex adverbialsNear();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} of$")]
  private static partial Regex adverbialsOf();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} off$")]
  private static partial Regex adverbialsOff();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} on$")]
  private static partial Regex adverbialsOn();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} onto$")]
  private static partial Regex adverbialsOnto();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} out$")]
  private static partial Regex adverbialsOut();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} outside$")]
  private static partial Regex adverbialsOutside();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} over$")]
  private static partial Regex adverbialsOver();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} past$")]
  private static partial Regex adverbialsPast();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} round$")]
  private static partial Regex adverbialsRound();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} thought$")]
  private static partial Regex adverbialsThought();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} through$")]
  private static partial Regex adverbialsThrough();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} throughout$")]
  private static partial Regex adverbialsThroughout();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} to$")]
  private static partial Regex adverbialsTo();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} together$")]
  private static partial Regex adverbialsTogether();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} toward$")]
  private static partial Regex adverbialsToward();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} towards$")]
  private static partial Regex adverbialsTowards();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} under$")]
  private static partial Regex adverbialsUnder();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} underneath$")]
  private static partial Regex adverbialsUnderneath();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} until$")]
  private static partial Regex adverbialsUntil();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} up$")]
  private static partial Regex adverbialsUp();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} upon$")]
  private static partial Regex adverbialsUpon();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} via$")]
  private static partial Regex adverbialsVia();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} with$")]
  private static partial Regex adverbialsWith();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} within$")]
  private static partial Regex adverbialsWithin();
  [GeneratedRegex(@"^(\b[a-zA-Z]+?)( [^ ]+?){0,4} without$")]
  private static partial Regex adverbialsWithout();
}
