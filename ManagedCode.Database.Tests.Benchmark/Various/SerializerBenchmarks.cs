using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace Benchmark;

[HtmlExporter]
[SimpleJob]
[MinColumn, MaxColumn, MeanColumn, MedianColumn, /*AllStatisticsColumn*/]
[RPlotExporter]
[MemoryDiagnoser]
[ThreadingDiagnoser]
[HardwareCounters(
    HardwareCounter.TotalCycles,
    HardwareCounter.TotalIssues,
    HardwareCounter.CacheMisses,
    HardwareCounter.Timer)]
public class SerializerBenchmarks
{

   public class localItem 
    {
        public int Field1 { get; set; }
        public long Field2 { get; set; }
        public string Field3 { get; set; }
        public float Field4 { get; set; }
        public double Field5 { get; set; }


        public static localItem Get()
        {
            return new localItem
            {
                Field1 = 15000,
                Field2 = 500600,
                Field3 = string.Join('-', Enumerable.Repeat(Guid.NewGuid(), 10)),
                Field4 = 2.33f,
                Field5 = 5.123d
            };
        }
    }
    
   /*
   [Benchmark]
   public void System_Text_Json_Serialize()
   {
       var item = localItem.Get(); 
       for (int i = 0; i < 1_000_000; i++)
       {
           var str = System.Text.Json.JsonSerializer.Serialize(item);
       }
   }
   
   [Benchmark]
   public void System_Text_Json_Deserialize()
   {
       var str = System.Text.Json.JsonSerializer.Serialize(localItem.Get());
       for (int i = 0; i < 1_000_000; i++)
       {
           var item = System.Text.Json.JsonSerializer.Deserialize<localItem>(str);
       }
   }
   
   [Benchmark]
   public void Newtonsoft_Json_Serialize()
   {
       var item = localItem.Get(); 
       for (int i = 0; i < 1_000_000; i++)
       {
           var str = Newtonsoft.Json.JsonConvert.SerializeObject(item);
       }
   }
   
   [Benchmark]
   public void Newtonsoft_Json_Deserialize()
   {
       var str = Newtonsoft.Json.JsonConvert.SerializeObject(localItem.Get());
       for (int i = 0; i < 1_000_000; i++)
       {
           var item = Newtonsoft.Json.JsonConvert.DeserializeObject<localItem>(str);
       }
   }
   
   [Benchmark]
   public void Newtonsoft_Bson_Serialize()
   {
       var item = localItem.Get(); 
       for (int i = 0; i < 1_000_000; i++)
       {
           var array = SerializeBson(item);
       }
   }
   
   [Benchmark]
   public void Newtonsoft_Bson_Deserialize()
   {
       var array = SerializeBson(localItem.Get());
       for (int i = 0; i < 1_000_000; i++)
       {
           var item = DeserializeBson(array);
       }
   }
   
   private localItem DeserializeBson(byte[] bytes)
   {
       using (MemoryStream ms = new MemoryStream(bytes))
       using (BsonDataReader reader = new BsonDataReader(ms))
       {
           JsonSerializer serializer = new JsonSerializer();
           return serializer.Deserialize<localItem>(reader);
       }
   }

   private byte[] SerializeBson(localItem entry)
   {
       using (MemoryStream ms = new MemoryStream())
       using (BsonDataWriter datawriter = new BsonDataWriter(ms))
       {
           JsonSerializer serializer = new JsonSerializer();
           serializer.Serialize(datawriter,  entry);
           return ms.ToArray();
       }
   }
   */
}

