using System;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Runtime.Serialization;

using System.Xml;
using Newtonsoft.Json;
using System.Runtime.InteropServices.WindowsRuntime;


namespace ReactiveFolder.Models.Util
{
	// Json.NET Conditional Serialization 
	// http://www.geekytidbits.com/conditional-serialization-with-json-net/





	public static class FileSerializeHelper 
	{


		public static void Save<T>(FileInfo file, T saveTarget)
			where T : class
		{
			try
			{
				_InserSave(file, saveTarget);
			}
			catch (Exception)
			{
				throw;
			}
		}

		public static void Save<T>(string file, T saveTarget) 
			where T : class
		{
			Save(new FileInfo(file), saveTarget);
        }
		
		public static void _InserSave<T>(FileInfo fileInfo, T saveTarget)
			where T : class
		{
			var format = ExtentionToContentFormat(fileInfo.Extension);

			// Note:
			// シリアライズの問題とファイル書き込みの問題を分離するため、
			// 一旦メモリストリームに書き出し、
			// 問題なければファイルへ書き込む処理へ。

			byte[] buf = null;
			
			switch (format)
			{
				case ContentFormat.Json:

					var str = ToJson<T>(saveTarget);
					buf = System.Text.Encoding.UTF8.GetBytes(str);

					break;
				case ContentFormat.Xml:

					DataContractSerializer serializer = new DataContractSerializer(typeof(T));
					XmlWriterSettings setting = new XmlWriterSettings();
					setting.Encoding = Encoding.UTF8;
					setting.Indent = true;

					using (MemoryStream ms = new MemoryStream())
					{
						using (var xmlwriter = XmlWriter.Create(ms, setting))
						{
							try
							{
								serializer.WriteObject(xmlwriter, saveTarget);
							}
							catch(Exception)
							{
								throw;
							}
                        }

						buf = ms.ToArray();
					}
					break;
				case ContentFormat.Binary:
					/*using (MemoryStream ms = new MemoryStream())
					{
						var bf = new BinaryWriter(ms);

						bf.Write(saveTarget);
					}
					*/
					throw new NotSupportedException();
				default:
					break;
			}

			
			if(false == fileInfo.Exists)
			{
				// Create後のStreamをDisposeしないとこの先で使えない
				using (var s = fileInfo.Create())
				{ }

			}

			if (buf == null || buf.Length == 0)
			{
				return;
			}


			using (var s = fileInfo.OpenWrite())
			{
				// 上書きモードで開かれるため、一旦ファイルの内容をクリア
				s.SetLength(0);

				// 書き込み
				s.Write(buf, 0, buf.Length);
//				await s.WriteAsync();
			}
		}

		static public T LoadAsync<T>(string filePath)
			where T : class
		{
			return LoadAsync<T>(new FileInfo(filePath));
		}

		static public T LoadAsync<T>(FileInfo fileInfo)
			where T : class
			
		{
			if (fileInfo == null || false == fileInfo.Exists)
			{
				return null;
			}
			var format = ExtentionToContentFormat(fileInfo.Extension);
			
			T returnObj = null;


			using (var stream = fileInfo.OpenText())
			{
				switch (format)
				{
					case ContentFormat.Json:
						var rawJson = stream.ReadToEnd();

						returnObj = FromJson<T>(rawJson);

						break;

					case ContentFormat.Xml:
						DataContractSerializer serializer =
							new DataContractSerializer(typeof(T));						
                        using (XmlReader xr = XmlReader.Create(stream))
						{
							try
							{
								returnObj = (T)serializer.ReadObject(xr);
							}
							catch(Exception e)
							{
								System.Diagnostics.Debug.WriteLine(e.Message);
								System.Diagnostics.Debugger.Break();
								throw;
							}
                        }

						break;

					case ContentFormat.Binary:
						/*
						FileStream fs = new FileStream(file.FullName,
							FileMode.Open,
							FileAccess.Read);
						try
						{
							BinaryFormatter bf = new BinaryFormatter();
							returnObj = bf.Deserialize(fs) as T;
						}
						finally
						{
							fs.Close();
						}
						*/
						break;

					default:
						throw new NotSupportedException();
				}
			}

			if (returnObj == null)
			{
				throw new Exception();
			}

			return returnObj;
		}


		static private ContentFormat ExtentionToContentFormat(string ext)
		{
			switch (ext)
			{
				case ".xml": return ContentFormat.Xml;
				case ".json": return ContentFormat.Json;

				case ".meta": return ContentFormat.Json;

				default:
					throw new NotSupportedException(ext);
			}
		}

		static private string ContentFormatExtention(string path, ContentFormat format)
		{
			var ext = System.IO.Path.GetExtension(path);

			switch (format)
			{
				case ContentFormat.Json:
					ext = "json";
					break;
				case ContentFormat.Xml:
					ext = "xml";
					break;
				case ContentFormat.Binary:
					ext = "bin";
					break;
				default:
					// ext no change.
					break;
			}

			return System.IO.Path.ChangeExtension(path, ext);
		}


		public static string ToJson<T>(T source)
			where T : class
		{
			var js = new JsonSerializer();
			js.TypeNameHandling = TypeNameHandling.Objects;
			js.Formatting = Newtonsoft.Json.Formatting.Indented;

			using (StringWriter sw = new StringWriter())
			{
				js.Serialize(sw, source);
				return sw.ToString();
			}
		}


		public static T FromJson<T>(string jsonText)
			where T : class
		{
			if (String.IsNullOrWhiteSpace(jsonText))
			{
				return null;
			}

			return JsonConvert.DeserializeObject<T>(jsonText, new JsonSerializerSettings()
			{
				TypeNameHandling = TypeNameHandling.Objects
			});
		}
		
	}


	public enum ContentFormat
	{
		Json,
		Xml,
		Binary
	}

}
