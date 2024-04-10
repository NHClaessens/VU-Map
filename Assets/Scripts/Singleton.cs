public abstract class Singleton<T> where T : Singleton<T>
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                // Note: this requires a public or protected constructor in the subclass
                _instance = System.Activator.CreateInstance<T>();
            }

            return _instance;
        }
    }

    protected Singleton()
    {
        if (_instance != null)
        {
            throw new System.Exception("Singleton instance has already been created.");
        }
    }
}
