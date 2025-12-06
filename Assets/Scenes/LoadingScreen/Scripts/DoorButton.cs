using UnityEngine;

public class DoorButton : MonoBehaviour
{
    public Transform door;       // الباب اللي هيلف
    public Transform glass;      // الأزاز اللي هيطلع وينزل
    public float rotationAngle = 90f;  // درجة دوران الباب على X
    public Vector3 glassDownPos;       // موقع الأزاز لما ينزل
    public float speed = 2f;           // سرعة الحركة

    private bool isOpen = false;       // حالة الباب والأزاز
    private Vector3 glassUpPos;        // موقع الأزاز الأصلي
    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        closedRotation = door.localRotation;
        openRotation = closedRotation * Quaternion.Euler(rotationAngle, 0, 0);
        glassUpPos = glass.localPosition;
    }

    public void PressButton()
    {
        isOpen = !isOpen;
    }

    void Update()
    {
        // دوران الباب
        door.localRotation = Quaternion.Slerp(door.localRotation,
                                              isOpen ? openRotation : closedRotation,
                                              Time.deltaTime * speed);

        // تحريك الأزاز
        glass.localPosition = Vector3.Lerp(glass.localPosition,
                                           isOpen ? glassDownPos : glassUpPos,
                                           Time.deltaTime * speed);
    }
}
