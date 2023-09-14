using UnityEngine;

namespace ThreeDeePongProto.Offline.Highscores
{
    public class HighscoreList : MonoBehaviour
    {
        [SerializeField] private Transform m_contentParentTransform;
        [SerializeField] private HighscoreEntrySlot m_listSlotPrefab;

        [SerializeField] private int m_highscoreSlots;
        [SerializeField] private MatchValues m_matchValues;

        private float m_slotHeight;

        //private IPersistentData m_persistentData = new SerializingData();
        //[SerializeField] private bool m_encryptionEnabled = false;

        private void Awake()
        {
            m_slotHeight = m_listSlotPrefab.GetComponent<RectTransform>().rect.height;

            for (int i = 0; i < m_highscoreSlots; i++)
            {
                //(Slot & Parent);
                HighscoreEntrySlot dataSlot = Instantiate(m_listSlotPrefab, m_contentParentTransform);
                RectTransform organizeRectTransform = dataSlot.GetComponent<RectTransform>();
                organizeRectTransform.anchoredPosition = new Vector2(0, -m_slotHeight * i);

                m_matchValues.ListIndex += 1;
                dataSlot.Initialise(m_matchValues);
                dataSlot.gameObject.SetActive(true);
            }
        }

        private void OnDisable()
        {
            m_matchValues.ListIndex = 0;
        }

        public void SortListBySetRounds()
        {
            Debug.Log("Kommt noch!");
        }

        public void SortListBySetMaxPoints()
        {
            Debug.Log("Kommt noch!");
        }

        public void SearchForPlayer()
        {
            Debug.Log("Kommt noch!");
        }

        public void SortListByDate()
        {
            Debug.Log("Kommt noch!");
        }

        public void SortByTotalTime()
        {
            Debug.Log("Kommt noch!");
        }
    }
}