﻿// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using Sharky.Chat;
//
//    var chatData = ChatData.FromJson(jsonString);

namespace Sharky.Chat
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class ChatData
    {
        [JsonProperty("triggers")]
        public List<string> Triggers { get; set; }

        [JsonProperty("responses")]
        public List<List<string>> Responses { get; set; }

        [JsonProperty("type")]
        public TypeEnum Type { get; set; }

        [JsonProperty("frequency")]
        public long Frequency { get; set; }

        [JsonProperty("command", NullValueHandling = NullValueHandling.Ignore)]
        public string Command { get; set; }

        [JsonProperty("chance", NullValueHandling = NullValueHandling.Ignore)]
        public string Chance { get; set; }
        [JsonProperty("LastResponseFrame", NullValueHandling = NullValueHandling.Ignore)]
        public int LastResponseFrame { get; set; }
    }

    public enum TypeEnum { Chat, Command, Grammar, Greeting, Laugh, GoodGame };

    public partial class ChatData
    {
        public static List<ChatData> FromJson(string json) => JsonConvert.DeserializeObject<List<ChatData>>(json, Sharky.Chat.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this List<ChatData> self) => JsonConvert.SerializeObject(self, Sharky.Chat.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                TypeEnumConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class TypeEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(TypeEnum) || t == typeof(TypeEnum?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "chat":
                    return TypeEnum.Chat;
                case "command":
                    return TypeEnum.Command;
                case "grammar":
                    return TypeEnum.Grammar;
                case "greeting":
                    return TypeEnum.Greeting;
                case "laugh":
                    return TypeEnum.Laugh;
                case "goodGame":
                    return TypeEnum.GoodGame;
            }
            throw new Exception("Cannot unmarshal type TypeEnum");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (TypeEnum)untypedValue;
            switch (value)
            {
                case TypeEnum.Chat:
                    serializer.Serialize(writer, "chat");
                    return;
                case TypeEnum.Command:
                    serializer.Serialize(writer, "command");
                    return;
                case TypeEnum.Grammar:
                    serializer.Serialize(writer, "grammar");
                    return;
                case TypeEnum.Greeting:
                    serializer.Serialize(writer, "greeting");
                    return;
                case TypeEnum.Laugh:
                    serializer.Serialize(writer, "laugh");
                    return;
            }
            throw new Exception("Cannot marshal type TypeEnum");
        }

        public static readonly TypeEnumConverter Singleton = new TypeEnumConverter();
    }
}
