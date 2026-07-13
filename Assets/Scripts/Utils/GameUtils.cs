using System.Collections;
using UnityEngine;

namespace Utils
{
    public static class GameUtils
    {
        public static float gameSpeedMultiplier = 1f;

        public static IEnumerator WaitDelayOrInput(float duration)
        {
            float timer = 0f;

            yield return new WaitForSeconds(0.1f);

            timer += 0.1f;

            while(timer < duration)
            {
                if(Input.anyKeyDown) break;

                timer += Time.deltaTime;

                yield return null;
            }
        }
        
        public static WaitForSeconds WaitForSecondsScaled(float baseTime)
        {
            return new WaitForSeconds(baseTime / gameSpeedMultiplier);
        }
    }
}
