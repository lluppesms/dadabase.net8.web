﻿//-----------------------------------------------------------------------
// <copyright file="SessionState.cs" company="Luppes Consulting, Inc.">
// Copyright 2024, Luppes Consulting, Inc. All rights reserved.
// </copyright>
// <summary>
// Session Variables
// </summary>
//-----------------------------------------------------------------------
namespace DadABase.Data;

/// <summary>
/// Session Variables
/// </summary>
[ExcludeFromCodeCoverage]
public class SessionState
{
    ///// <summary>
    ///// Chat Instructions
    ///// </summary>
    //public string ChatInstructions = "You are an AI assistant that helps people find information.";

    ///// <summary>
    ///// Chat Temperature - Integer 0-100
    ///// </summary>
    //public int ChatTemperature
    //{
    //    get
    //    {
    //        return _tempInt;
    //    }
    //    set
    //    {
    //        _tempInt = value;
    //        ChatTemperatureDec = (decimal)value * 0.01m;
    //    }
    //}
    //private int _tempInt = 50;

    ///// <summary>
    ///// Chat Temperature - Decimal 0.0-1.0
    ///// </summary>
    //public decimal ChatTemperatureDec = 0.50m;

    ///// <summary>
    ///// Chat Token Count
    ///// </summary>
    //public int ChatTokenValue
    //{
    //    get
    //    {
    //        return _tokenInt;
    //    }
    //    set
    //    {
    //        _tokenInt = value;
    //        _tokenCost = (decimal)value * _tokenCostMultiplier;
    //        ChatTokenCost = $"{_tokenCost:0.0000}";
    //    }
    //}
    //private int _tokenInt = 1000;
    //private decimal _tokenCost = 0.002m;
    //private readonly decimal _tokenCostMultiplier = 0.000002m; // ~ $0.002 / 1K tokens

    ///// <summary>
    ///// Chat Token Cost
    ///// </summary>
    //public string ChatTokenCost = "0.002";

    ///// <summary>
    ///// Chat Selected Language Model
    ///// </summary>
    //public string ChatSelectedModel = Constants.LanguageModelType.gpt35turbo;

    ///// <summary>
    ///// Chat Selected Personality Model
    ///// </summary>
    //public string Personality = string.Empty;

    ///// <summary>
    ///// Chat Current Message
    ///// </summary>
    //public ChatMessage ChatCurrentMessage = new();

    ///// <summary>
    ///// Chat Message History
    ///// </summary>
    //public List<MessageBubble> ChatMessageHistory = new();

    /// <summary>
    /// Constructor
    /// </summary>
    public SessionState()
    {

    }
    /// <summary>
    /// Constructor
    /// </summary>
    public SessionState(string chatSelectedModel)
    {
        //ChatSelectedModel = chatSelectedModel;
    }

    ///// <summary>
    ///// Returns LastMessage plus a personality setting
    ///// </summary>
    //public string MessagePlusPersonality
    //{
    //    get
    //    {
    //        var personality = ChatPersonality.Personalities.Where(p => p.Name == Personality).FirstOrDefault();
    //        return personality == null ? ChatCurrentMessage.LastRequest : $"{personality.Prompt} {ChatCurrentMessage.LastRequest}";
    //    }
    //}
}
