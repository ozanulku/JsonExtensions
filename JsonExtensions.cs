using Newtonsoft.Json.Linq;
using System;
using System.Data.SqlClient;


public static class JsonExtensions
{

	public static bool IsNull(this JToken token)
	{
		return (token == null) ||
			   (token.Type == JTokenType.Null);
	}

	public static bool IsNullOrEmpty(this JToken token)
	{
		return (token == null) ||
			   (token.Type == JTokenType.Array && !token.HasValues) ||
			   (token.Type == JTokenType.Object && !token.HasValues) ||
			   (token.Type == JTokenType.String && token.ToString() == String.Empty) ||
			   (token.Type == JTokenType.Null);
	}

	public static object GetSqlValue(this JToken token)
	{
		if (token == null) return DBNull.Value;

		switch (token.Type)
		{
			case JTokenType.Boolean:
				return token.Value<bool>();

			case JTokenType.Date:
				return token.Value<DateTime>().ToLocalTime();

			case JTokenType.Float:
				return token.Value<double>();

			case JTokenType.Integer:
				return token.Value<int>();

			case JTokenType.Null:
				return DBNull.Value;

			case JTokenType.String:
				return token.Value<string>();

			case JTokenType.Array:
			case JTokenType.Bytes:
			case JTokenType.Comment:
			case JTokenType.Constructor:
			case JTokenType.Guid:
			case JTokenType.None:
			case JTokenType.Object:
			case JTokenType.Raw:
			case JTokenType.Property:
			case JTokenType.TimeSpan:
			case JTokenType.Undefined:
			case JTokenType.Uri:
			default:
				throw new NotImplementedException("GetSqlValue method is not implemented for JToken.Type : " + token.Type.ToString());
		}
	}

	public static object GetSqlValue(this JToken token, string key)
	{
		return token[key].GetSqlValue();
	}

	public static JObject GetJObjectFromReader(this SqlDataReader reader, string dateFormat = "yyyy.MM.dd", string dateTimeFormat = "yyyy.MM.dd HH:mm:ss")
	{
		JObject jo = new JObject();
		for (int i = 0; i < reader.VisibleFieldCount; i++)
		{
			if (reader.IsDBNull(i))
			{
				jo.Add(reader.GetName(i), null);
				continue;
			}
			switch (reader.GetDataTypeName(i))
			{
				case "bit":
					jo.Add(reader.GetName(i), reader.GetBoolean(i));
					break;

				case "bigint":
					jo.Add(reader.GetName(i), reader.GetInt64(i));
					break;

				case "int":
					jo.Add(reader.GetName(i), reader.GetInt32(i));
					break;

				case "smallint":
					jo.Add(reader.GetName(i), reader.GetInt16(i));
					break;

				case "tinyint":
					jo.Add(reader.GetName(i), reader.GetByte(i));
					break;

				case "date":
					jo.Add(reader.GetName(i), reader.GetDateTime(i).ToString(dateFormat));
					break;

				case "datetime":
					jo.Add(reader.GetName(i), reader.GetDateTime(i).ToString(dateTimeFormat));
					break;

				case "float":
					jo.Add(reader.GetName(i), reader.GetDouble(i));
					break;

				case "uniqueidentifier":
					jo.Add(reader.GetName(i), reader.GetGuid(i));
					break;

				default:
					jo.Add(reader.GetName(i), reader[i].ToString());
					break;
			}
		}
		return jo;
	}

	public static JArray GetJArrayFromReader(this SqlDataReader reader, string dateFormat = "yyyy.MM.dd", string dateTimeFormat = "yyyy.MM.dd HH:mm:ss")
	{
		JArray arr = new JArray();
		while (reader.Read())
		{
			arr.Add(GetJObjectFromReader(reader, dateFormat, dateTimeFormat));
		}
		return arr;
	}
}
