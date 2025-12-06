using UnityEngine;

public class EnvironmentLoop : MonoBehaviour
{
    public float speed = 5f;               // سرعة حركة البيئة
    public float resetPositionZ = -50f;    // لحد ما توصل فين وترجع تاني
    public float startPositionZ = 50f;     // أول مكان ترجع له

    void Update()
    {
        // التحريك للخلف
        transform.Translate(Vector3.back * speed * Time.deltaTime);

        // لو وصلت للنقطة المطلوبة ترجع تاني
        if (transform.position.z <= resetPositionZ)
        {
            Vector3 newPos = transform.position;
            newPos.z = startPositionZ;
            transform.position = newPos;
        }
    }
}
