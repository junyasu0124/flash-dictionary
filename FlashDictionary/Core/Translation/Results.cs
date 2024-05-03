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

  public void Add((string Original, string[] Meaning) item)
  {
    Items.Add(new(item));
  }
  public void AddRange(List<(string Original, string[] Meaning)> items)
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
    var foregroundInBracket = new SolidColorBrush(new UISettings().GetColorValue(UIColorType.AccentLight2));

    List<Paragraph> paragraphs = [];

    foreach (var meaning in Meanings)
    {
      List<Range> bracketPairs = [];

      char? targetLeft = null;
      char? targetRight = null;
      int leftBracketIndex = -1;
      int numberOfLeftBrackets = 0;
      for (var i = 0; i < meaning.Length; i++)
      {
        if (numberOfLeftBrackets == 0)
        {
          var leftIndex = Array.IndexOf(leftBrackets, meaning[i]);
          if (leftIndex == -1)
            continue;
          targetLeft = meaning[i];
          targetRight = rightBrackets[leftIndex];
          leftBracketIndex = i;
          numberOfLeftBrackets = 1;
        }
        else
        {
          if (meaning[i] == targetLeft)
          {
            numberOfLeftBrackets++;
          }
          else if (meaning[i] == targetRight)
          {
            numberOfLeftBrackets--;
            if (numberOfLeftBrackets == 0)
            {
              bracketPairs.Add(new Range(leftBracketIndex, i));
              targetRight = null;
            }
          }
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
