using System.Collections.Generic;

public class PlayerProfileManager : PersistentSingleton<PlayerProfileManager>
{
    //Central List for PlayerProfiles 1-4
    public List<PlayerProfileData> PlayerProfiles { get; private set; }

    private const string m_playerProfiles = "playerProfiles.json";

    private IPersistentData<List<PlayerProfileData>> m_saveSystem;

    protected override void Awake()
    {
        base.Awake();
        m_saveSystem = new SerializingData<List<PlayerProfileData>>(m_playerProfiles);
        LoadProfiles();
    }

    public void LoadProfiles()
    {
        PlayerProfiles = m_saveSystem.Load();

        //Creates 4 new PlayerProfiles, if non saveData exists.
        if (PlayerProfiles == null || PlayerProfiles.Count == 0)
        {
            PlayerProfiles = new List<PlayerProfileData>();
            for (int i = 0; i < 4; i++)
            {
                PlayerProfiles.Add(new PlayerProfileData(i));
            }
        }
    }

    public void SaveProfiles()
    {
        m_saveSystem.Save(PlayerProfiles);
    }
}