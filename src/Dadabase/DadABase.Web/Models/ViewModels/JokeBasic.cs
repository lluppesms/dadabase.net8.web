﻿//-----------------------------------------------------------------------
// <copyright file="JokeBasic.cs" company="Luppes Consulting, Inc.">
// Copyright 2024, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Basic Joke ViewModel
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Data;

/// <summary>
/// Basic Joke ViewModel
/// </summary>
public class JokeBasic
{
    /// <summary>
    /// Joke Category
    /// </summary>
    //[JsonProperty("Category")]
    public string Category { get; set; }

    /// <summary>
    /// Joke
    /// </summary>
    //[JsonProperty("Joke")]
    public string Joke { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public JokeBasic()
    {
        Joke = string.Empty;
        Category = string.Empty;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public JokeBasic(string jokeTxt, string jokeCategoryTxt)
    {
        Joke = jokeTxt;
        Category = jokeCategoryTxt;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public JokeBasic(string jokeTxt, string jokeCategoryTxt, string imageText)
    {
        Joke = jokeTxt;
        Category = jokeCategoryTxt;
        //ImageText = imageText;
    }
}

/// <summary>
/// Basic Category without the extraneous fields
/// </summary>
public class CategoryBasic
{
    /// <summary>
    /// Joke Category
    /// </summary>
    public string Category { get; set; }
}

/// <summary>
/// List of basic jokes
/// </summary>
public class JokeBasicList
{
    /// <summary>
    /// List of basic jokes
    /// </summary>
    public List<JokeBasic> Jokes { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public JokeBasicList()
    {
        Jokes = [];
    }
}
