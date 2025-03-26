namespace _Project.Scripts.Persistence {
    public interface ISerializer {
        string Serialize<T>(T obj);
        T Deserialize<T>(string json);
    }
}