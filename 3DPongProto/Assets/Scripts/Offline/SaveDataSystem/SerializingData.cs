using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

/// <summary>
/// A robust serialization class that handles saving and loading data to/from a JSON file.
/// It uses a temporary/backup file strategy to prevent data loss during writes.
/// </summary>
/// <typeparam name="T">The data type to serialize. Must be a class with a parameterless constructor.</typeparam>
public class SerializingData<T> : IPersistentData<T> where T : class, new()
{
    private readonly string m_filePath;
    private readonly string m_backupFilePath;
    private readonly string m_tempFilePath;
    private readonly bool m_useEncryption;

    private const string m_saveDataFolderPath = "/SaveData/";

    private const string m_KEY = "Yx/P5QVTRuUt55p82QNnkI1LXbXM4/qsxM9P7eihc0o=";
    private const string m_IV = "L5j2EvGAywqpH86whhvjWA=="; //InitializationVector.

    /// <summary>
    /// Initializes the serializer for a specific file.
    /// </summary>
    /// <param name="_fileName">The name of the file, e.g., "bindings.json".</param>
    /// <param name="_subFolder">Optional subfolder within Application.persistentDataPath.</param>
    /// <param name="_useEncryption">Whether to encrypt the saved data.</param>
    public SerializingData(string _fileName, string _subFolder = m_saveDataFolderPath, bool _useEncryption = false)
    {
        string directoryPath = Path.Combine(Application.persistentDataPath, _subFolder.TrimStart('/'));

        //Ensure the directory exists.
        Directory.CreateDirectory(directoryPath);

        m_filePath = Path.Combine(directoryPath, _fileName);
        m_backupFilePath = m_filePath + ".bak";
        m_tempFilePath = m_filePath + ".tmp";
        this.m_useEncryption = _useEncryption;
    }

    /// <summary>
    /// Loads the data from the JSON file.
    /// Will attempt to load from a backup if the main file is corrupt.
    /// Will return a new object if no valid file is found.
    /// </summary>
    public T Load()
    {
        //Prioritize the main file.
        if (File.Exists(m_filePath))
        {
            T data = ReadAndDeserialize(m_filePath);
            if (data != null)
            {
                //Debug.Log("Data exists and is loaded.");
                return data;
            }
        }

        //If main file failed or doesn't exist, try the backup.
        if (File.Exists(m_backupFilePath))
        {
            Debug.LogWarning("Main save file was corrupt or missing. Attempting to load from backup.");
            T data = ReadAndDeserialize(m_backupFilePath);
            if (data != null)
            {
                return data; //Successfully loaded from backup.
            }
        }

        //If all else fails, return a new default object.
        Debug.Log("No valid save file found. Creating new default data.");
        return new T();
    }

    /// <summary>
    /// Saves the data to a JSON file using a fail-safe atomic write.
    /// </summary>
    public void Save(T data)
    {
        try
        {
            string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);

            //1. Write to a temporary file.
            if (m_useEncryption)
            {
                //Encryption logic writes directly to the temp file _stream.
                using FileStream tempStream = File.Create(m_tempFilePath);
                WriteEncryptedData(jsonData, tempStream);
            }
            else
            {
                File.WriteAllText(m_tempFilePath, jsonData);
            }

            //2. Replace the old backup with the current main file (if it exists).
            if (File.Exists(m_filePath))
            {
                File.Replace(m_tempFilePath, m_filePath, m_backupFilePath);
            }
            else
            {
                //3. If no main file exists, just rename the temp file.
                File.Move(m_tempFilePath, m_filePath);
            }

            //Optional: Clean up the temporary backup file created by File.Replace.
            if (File.Exists(m_backupFilePath))
            {
                File.Delete(m_backupFilePath);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save data to {m_filePath}. Reason: {e.Message}\n{e.StackTrace}");
        }
    }

    private T ReadAndDeserialize(string path)
    {
        try
        {
            if (m_useEncryption)
            {                
                return ReadEncryptedData(path);  //Decryption logic reads from the path.
            }
            else
            {
                string jsonData = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<T>(jsonData);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to read or deserialize file at {path}. Reason: {e.Message}");
            return null;
        }
    }

    private void WriteEncryptedData(string _jsonData, FileStream _stream)
    {
        using Aes aesEncryptionProvider = Aes.Create();
        //Has to be commented out, to use the 2 Debug.Logs below, to get new Key and IV.___
        aesEncryptionProvider.Key = Convert.FromBase64String(m_KEY);
        aesEncryptionProvider.IV = Convert.FromBase64String(m_IV);
        //_________________________________________________________________________________

        using ICryptoTransform encryptor = aesEncryptionProvider.CreateEncryptor();
        using CryptoStream cryptoStream = new CryptoStream(_stream, encryptor, CryptoStreamMode.Write);
        using StreamWriter writer = new StreamWriter(cryptoStream);
        writer.Write(_jsonData);

        //using ICryptoTransform cryptoTransform = aesEncryptionProvider.CreateEncryptor();
        //using CryptoStream cryptoStream = new CryptoStream(_stream, cryptoTransform, CryptoStreamMode.Write);

        ////OneTime use for Key and IV creation set, on top of this class:_____________________________
        ////Debug.Log($" Key: {Convert.ToBase64String(aesEncryptionProvider.Key)}");
        ////Debug.Log($"InitializeBasic Vector: {Convert.ToBase64String(aesEncryptionProvider.IV)}");
        ////___________________________________________________________________________________________

        ////Encoding.ASCII from the Tutorial 'https://www.youtube.com/watch?v=mntS45g8OK4'.
        ////TODO: Place to add more Serialize options. (switch with _fileFormat?)
        //cryptoStream.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_data, Formatting.Indented)));
    }

    private T ReadEncryptedData(string _path)
    {
        byte[] fileBytes = File.ReadAllBytes(_path);

        using Aes aesDecryptionProvider = Aes.Create();
        aesDecryptionProvider.Key = Convert.FromBase64String(m_KEY);
        aesDecryptionProvider.IV = Convert.FromBase64String(m_IV);

        using ICryptoTransform decryptor = aesDecryptionProvider.CreateDecryptor(aesDecryptionProvider.Key, aesDecryptionProvider.IV);
        using MemoryStream memoryStream = new MemoryStream(fileBytes);
        using CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using StreamReader reader = new StreamReader(cryptoStream);
        string decryptedJson = reader.ReadToEnd();
        return JsonConvert.DeserializeObject<T>(decryptedJson);

        //using ICryptoTransform cryptoTransform = aesDecryptionProvider.CreateDecryptor(aesDecryptionProvider.Key, aesDecryptionProvider.IV);
        //using MemoryStream decryptionStream = new(fileBytes);
        //using CryptoStream cryptoStream = new(decryptionStream, cryptoTransform, CryptoStreamMode.Read);

        //using StreamReader reader = new(cryptoStream);
        //string decryptedData = reader.ReadToEnd();

        //Debug.Log($"Decrypted data. On any error check used KEY and/or Initialization Vector: {decryptedData}.");
        ////TODO: Place to add more Deserialize options. (switch with _fileFormat?)
        //return JsonConvert.DeserializeObject<T>(decryptedData);
    }
}