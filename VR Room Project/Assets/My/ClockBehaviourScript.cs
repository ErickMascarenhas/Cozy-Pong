using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class ClockBehaviourScript : MonoBehaviour
{
    System.DateTime dt1;
    public GameObject handHour;
    public GameObject handMinute;
    public GameObject handSecond;
    /*
     * com gametime
    public int gameHours;
    public int gameMinutes;
    public float gameSeconds;
    */

    private void Start()
    {
        dt1 = System.DateTime.Now;
        /*
         * com gametime
        gameHours = dt1.Hour;
        gameMinutes = dt1.Minute;
        gameSeconds = dt1.Second;
        */
    }

    void Update()
    {
        dt1 = System.DateTime.Now;
        /*
         * com gametime
        gameSeconds += Time.deltaTime;
        if (gameSeconds > 59)
        {
            gameSeconds = 0;
            gameMinutes++;
            if (gameMinutes > 59)
            {
                gameMinutes = 0;
                gameHours++;
                if (gameHours > 23)
                {
                    gameHours = 0;
                }
            }
        }

        */
        int hours = dt1.Hour;
        int minutes = dt1.Minute;
        int seconds = dt1.Second;

        /*
         * sem gametime, mas tava com flick
        Vector3 tVec = handHour.transform.localEulerAngles;
        tVec.x = hours * 360 / 12;
        handHour.transform.localEulerAngles = tVec;

        tVec = handMinute.transform.localEulerAngles;
        tVec.x = minutes * 360 / 60;
        handMinute.transform.localEulerAngles = tVec;

        tVec = handSecond.transform.localEulerAngles;
        tVec.x = seconds * 360 / 60;
        handSecond.transform.localEulerAngles = tVec;
        */

        handHour.transform.localRotation = Quaternion.Euler(hours * 360f / 12f, 0, 0);
        handMinute.transform.localRotation = Quaternion.Euler(minutes * 360f / 60f, 0, 0);
        handSecond.transform.localRotation = Quaternion.Euler(seconds * 360f / 60f, 0, 0);

        /*
         * com gametime
        Vector3 tVec = handHour.transform.localEulerAngles;
        tVec.x = (gameHours * 360 / 12) + (gameMinutes * 360 / 720);
        handHour.transform.localEulerAngles = tVec;

        tVec = handMinute.transform.localEulerAngles;
        tVec.x = (gameMinutes * 360 / 60) + (gameSeconds * 360 / 3600);
        handMinute.transform.localEulerAngles = tVec;

        tVec = handSecond.transform.localEulerAngles;
        tVec.x = (gameSeconds * 360 / 60);
        handSecond.transform.localEulerAngles = tVec;
        */
    }
}
