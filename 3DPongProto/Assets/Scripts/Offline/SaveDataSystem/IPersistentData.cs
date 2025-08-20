/// <summary>
/// Provides a simplified and robust interface for persisting a single data object.
/// </summary>
/// <typeparam name="T">The type of the data object to persist.</typeparam>
public interface IPersistentData<T> where T : class, new()
{
    /// <summary>
    /// Saves the data object to its configured persistent location.
    /// </summary>
    /// <param name="data">The data object to save.</param>
    void Save(T data);

    /// <summary>
    /// Loads the data object from its configured persistent location.
    /// </summary>
    /// <returns>The loaded data object. Returns a new default object if loading fails or no file exists.</returns>
    T Load();
}