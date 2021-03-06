﻿using System;

namespace Kusto.Language.Editor
{
    public enum CompletionKind
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// A language keyword
        /// </summary>
        Keyword,

        /// <summary>
        /// Punctuation like () {} ; , : 
        /// </summary>
        Punctuation,

        /// <summary>
        /// Other syntax item
        /// </summary>
        Syntax,

        /// <summary>
        /// An identifier
        /// </summary>
        Identifier,

        /// <summary>
        /// A literal
        /// </summary>
        Literal,

        /// <summary>
        /// A keyword that starts a scalar expression
        /// </summary>
        ScalarPrefix,

        /// <summary>
        /// A keyword that starts a tabular expression
        /// </summary>
        TabularPrefix,

        /// <summary>
        /// A keyword that follows a tabular expression
        /// </summary>
        TabularSuffix,

        /// <summary>
        /// A keyword that starts a query operator
        /// </summary>
        QueryPrefix,

        /// <summary>
        /// A keyword that starts a control command
        /// </summary>
        CommandPrefix,

        /// <summary>
        /// An infix scalar operator
        /// </summary>
        ScalarInfix,

        /// <summary>
        /// The name of a render chart type
        /// </summary>
        RenderChart,

        // symbols

        /// <summary>
        /// The name of a column
        /// </summary>
        Column,

        /// <summary>
        /// The name of a table
        /// </summary>
        Table,

        /// <summary>
        /// The name of a scalar function
        /// </summary>
        ScalarFunction,

        /// <summary>
        /// The name of a tabular function
        /// </summary>
        TabularFunction,

        /// <summary>
        /// The name of an aggregate function
        /// </summary>
        AggregateFunction,

        /// <summary>
        /// The name of a parameter
        /// </summary>
        Parameter,

        /// <summary>
        /// The name of a variable
        /// </summary>
        Variable,

        /// <summary>
        /// The name of a database
        /// </summary>
        Database,

        /// <summary>
        /// The name of a cluster
        /// </summary>
        Cluster
    }
}