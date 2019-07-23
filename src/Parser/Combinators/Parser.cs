﻿using Kusto.Language.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Kusto.Language.Parsing
{
    /// <summary>
    /// A parser combinator that can scan, search and parse input.
    /// When parsing, may produce zero or more output values.
    /// </summary>
    [DebuggerDisplay("{GetType().Name}: {Description}")]
    public abstract class Parser<TInput>
    {
        /// <summary>
        /// Create a shallow copy of this grammar rule
        /// </summary>
        protected abstract Parser<TInput> Clone();

        /// <summary>
        /// The name of the grammar element.
        /// Most grammar elements will not have names.
        /// </summary>
        public string Tag { get; private set; } = string.Empty;

        /// <summary>
        /// Creates a copy of this <see cref="Parser{TInput}"/> with the tag specified.
        /// </summary>
        public Parser<TInput> WithTag(string tag)
        {
            tag = tag ?? string.Empty;

            if (tag != this.Tag)
            {
                var clone = this.Clone();
                clone.Tag = tag;
                clone.Annotations = this.Annotations;
                clone.IsHidden = this.IsHidden;
                return clone;
            }
            else
            {
                return this;
            }
        }

        /// <summary>
        /// Annotations on the grammar element.
        /// </summary>
        public IReadOnlyList<object> Annotations { get; private set; } = EmptyReadOnlyList<object>.Instance;

        /// <summary>
        /// Creates a copy of this <see cref="Parser{TInput}"/> with the annotations specified.
        /// </summary>
        public Parser<TInput> WithAnnotations(IEnumerable<object> annotations)
        {
            var list = annotations.ToReadOnly();

            if (this.Annotations != list)
            {
                var clone = this.Clone();
                clone.Annotations = list;
                clone.Tag = this.Tag;
                clone.IsHidden = this.IsHidden;
                return clone;
            }
            else
            {
                return this;
            }
        }

        private string description;

        /// <summary>
        /// A description of the grammar element.
        /// </summary>
        public string Description
        {
            get
            {
                if (this.description == null)
                {
                    if (this.Tag == null)
                    {
                        this.description = GrammarBuilder.BuildGrammar(this);
                    }
                    else
                    {
                        this.description = GrammarBuilder.BuildGrammar(this.WithTag(null));
                    }
                }

                return this.description;
            }
        }

        /// <summary>
        /// True if the grammar element is hidden from searching.
        /// </summary>
        public bool IsHidden { get; private set; } = false;

        /// <summary>
        /// Creates a copy of this <see cref="Parser{TInput}"/> with the IsHidden property specified.
        /// </summary>
        public Parser<TInput> WithIsHidden(bool isHidden)
        {
            if (isHidden != this.IsHidden)
            {
                var clone = this.Clone();
                clone.IsHidden = isHidden;
                clone.Tag = this.Tag;
                clone.Annotations = this.Annotations;
                return clone;
            }
            else
            {
                return this;
            }
        }

        /// <summary>
        /// Creates a copy of the <see cref="Parser{TInput}"/> that is hidden from searching.
        /// </summary>
        public Parser<TInput> Hide() => this.WithIsHidden(true);

        /// <summary>
        /// True if the parser can succeed with no input consumed.
        /// </summary>
        public virtual bool IsOptional => false;

        /// <summary>
        /// Invokes the corresponding <see cref="ParserVisitor{TInput}"/> visit method.
        /// </summary>
        public abstract void Accept(ParserVisitor<TInput> visitor);

        /// <summary>
        /// Invokes the corresponding <see cref="ParserVisitor{TInput, TResult}"/> visit method.
        /// </summary>
        public abstract TResult Accept<TResult>(ParserVisitor<TInput, TResult> visitor);

        /// <summary>
        /// Parses input source items and produces zero or more output items.
        /// </summary>
        public abstract int Parse(Source<TInput> input, int inputStart, List<object> output, int outputStart);

        /// <summary>
        /// Returns the number of source items that successfully match this grammar, or a negative number indicating failure.
        /// </summary>
        public abstract int Scan(Source<TInput> input, int inputStart);

        /// <summary>
        /// Searches the grammar and invokes the action for each grammar element that is considered.
        /// </summary>
        /// <param name="input">The input source.</param>
        /// <param name="inputStart">The starting offset within the input source.</param>
        /// <param name="prevWasMissing">True if the previous rule considered was required and determined to be missing.</param>
        /// <param name="action">The action to take each time the search considers a grammar element (parser).</param>
        public SearchResult Search(Source<TInput> input, int inputStart, bool prevWasMissing, SearchAction<TInput> action)
        {
            return SafeSearcher.SearchSafe(this, input, inputStart, prevWasMissing, action);
        }

        /// <summary>
        /// Searches the grammar and invokes the action for each grammar element that is considered.
        /// </summary>
        public SearchResult Search(Source<TInput> source, SearchAction<TInput> action) =>
            Search(source, 0, false, action);
    }

    public struct ParseResult<TOutput>
    {
        /// <summary>
        /// The number of input items consumed by the parser or a negative number if the parsing failed.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// The single produced result of the parser.
        /// </summary>
        public TOutput Value { get; }

        public ParseResult(int length, TOutput value)
        {
            this.Length = length;
            this.Value = value;
        }
    }

    /// <summary>
    /// A parser that will produce exactly one output item if it succeeds.
    /// </summary>
    public abstract class Parser<TInput, TOutput> : Parser<TInput>
    {
        /// <summary>
        /// Parses input source items and produces a single output item.
        /// </summary>
        public abstract ParseResult<TOutput> Parse(Source<TInput> input, int inputStart);

        public new Parser<TInput, TOutput> WithTag(string tag) => (Parser<TInput, TOutput>)base.WithTag(tag);
        public new Parser<TInput, TOutput> WithAnnotations(IEnumerable<object> annotations) => (Parser<TInput, TOutput>)base.WithAnnotations(annotations);
        public new Parser<TInput, TOutput> WithIsHidden(bool isHidden) => (Parser<TInput, TOutput>)base.WithIsHidden(isHidden);
        public new Parser<TInput, TOutput> Hide() => this.WithIsHidden(true);

        public Parser<TInput, TNewOutput> Cast<TNewOutput>() =>
            Parsers<TInput>.Rule(this, o => (TNewOutput)(object)o).WithTag(this.Tag);
    }

    /// <summary>
    /// A parser that is allowed on the right side of an Apply.
    /// </summary>
    public struct RightParser<TInput, TOutput>
    {
        internal Parser<TInput, TOutput> Parser { get; }

        internal RightParser(Parser<TInput, TOutput> parser)
        {
            this.Parser = parser;
        }

        public RightParser<TInput, TOutput> WithTag(string tag) => new RightParser<TInput, TOutput>(this.Parser.WithTag(tag));
        public RightParser<TInput, TOutput> WithAnnotations(IEnumerable<object> annotations) => new RightParser<TInput, TOutput>(this.Parser.WithAnnotations(annotations));
        public RightParser<TInput, TOutput> WithIsHidden(bool isHidden) => new RightParser<TInput, TOutput>(this.Parser.WithIsHidden(isHidden));
        public RightParser<TInput, TOutput> Hide() => this.WithIsHidden(true);
    }

    public static class ParserExtensions
    {
        /// <summary>
        /// Parses the text into the output list.
        /// </summary>
        public static int Parse(this Parser<char> parser, string text, List<object> output)
        {
            return parser.Parse(new TextSource(text), 0, output, 0);
        }

        /// <summary>
        /// Parses the text and returns a single output value in <see cref="ParseResult{TOutput}"/>.
        /// </summary>
        public static ParseResult<TOutput> Parse<TOutput>(this Parser<char, TOutput> parser, string text)
        {
            return parser.Parse(new TextSource(text), 0);
        }

        /// <summary>
        /// Returns true if the parser successfully parses the text. 
        /// Returns the produced value as an out parameter.
        /// </summary>
        public static bool TryParse<TOutput>(this Parser<char, TOutput> parser, string text, out TOutput value)
        {
            var result = parser.Parse(text);
            value = result.Value;
            return result.Length > 0;
        }
    }

    /// <summary>
    /// A parsed value and its source offset.
    /// </summary>
    public struct OffsetValue<TValue>
    {
        public readonly int Offset;
        public readonly TValue Value;

        public OffsetValue(int offset, TValue value)
        {
            this.Offset = offset;
            this.Value = value;
        }
    }
}