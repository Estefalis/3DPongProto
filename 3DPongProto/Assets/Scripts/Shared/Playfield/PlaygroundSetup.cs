using UnityEngine;

public class PlaygroundSetup : MonoBehaviour
{
    [SerializeField] private GameObject m_playGround;
    [SerializeField] private Transform m_middleLine;

    [SerializeField] private float m_fieldWidthScale = 0.1f, m_fieldLengthScale = 0.1f, m_middleLineScale = 0.00025f;   //Or non SF const float.

    public void SetScale(float _fieldWidth, float _fieldLength)
    {
        if(m_playGround == null) return;    //Plus warning.
        {
            //Scale the playfield.
            transform.localScale = new Vector3(_fieldWidth * m_fieldWidthScale, transform.localScale.y, _fieldLength * m_fieldLengthScale);

            //Scale the middleLine.
            if (m_middleLine != null)
            {
                Vector3 lineScale = m_middleLine.localScale;
                lineScale.z = _fieldLength * m_middleLineScale;
                m_middleLine.localScale = lineScale;
            }
        }
    }
}