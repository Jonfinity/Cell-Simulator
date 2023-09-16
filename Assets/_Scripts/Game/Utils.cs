using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Utils : MonoBehaviour
{
    public const float SCALE_MULTIPLIER = 1.15f;//1.3f
    public const int MASS_SCALE_MULTIPLIER = 6;
    
    public const int MIN_SHOT_MASS_SIZE = 4;
    public const int MAX_SHOT_MASS_SIZE = 69;

    private static System.DateTime epochStart;

    public static Color mapBrightColor;
    public static Color mapDarkColor;

    public static Color playingHudTextBrightColor;
    public static Color playingHudTextDarkColor;

    public static Color brightJoystickBackgroundColor;
    public static Color darkJoystickBackgroundColor;
    public static Color brightJoystickHandleColor;
    public static Color darkJoystickHandleColor;

    public static Vector3 scoreWithNamePosition;
    public static Vector3 scoreNoNamePosition;

    public static Vector3 levelWithNamePosition;
    public static Vector3 levelNoNamePosition;

    public static Vector2 foodScale = new Vector2(Food.SCALE, Food.SCALE);
    public static Vector2 virusScale = new Vector2(Virus.SCALE, Virus.SCALE);

    private void Awake()
    {
        epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);

        mapBrightColor = new Color(0f, 0f, 0f, 0.12f);
        mapDarkColor = new Color(1f, 1f, 1f, 0.12f);

        playingHudTextBrightColor = new Color(1, 1, 1, 0.85f);
        playingHudTextDarkColor = new Color(0, 0, 0, 0.45f);

        brightJoystickBackgroundColor = new Color(1, 1, 1, 0.12f);
        darkJoystickBackgroundColor = new Color(0, 0, 0, 0.12f);

        brightJoystickHandleColor = new Color(1, 1, 1, 0.14f);
        darkJoystickHandleColor = new Color(0, 0, 0, 0.14f);

        scoreWithNamePosition = new Vector3(0, -1.6f, 0);
        scoreNoNamePosition = new Vector3(0, -0f, 0);

        levelWithNamePosition = new Vector3(0, -3.0f, 0);
        levelNoNamePosition = new Vector3(0, -1.4f, 0);
    }

    public static int secondsSinceEpoch()
    {
        return (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
    }
    
    public static long millisecondsSinceEpoch()
    {
        return (long)(System.DateTime.UtcNow - epochStart).TotalMilliseconds;
    }

    public static bool isColorAlmostBlack(Color color)
    {
        return (color.r < 0.2 && color.g < 0.2 && color.b < 0.2 && color.a == 1);
    }

    public static bool isColorAlmostWhite(Color color)
    {
        return (color.r > 0.8 && color.g > 0.8 && color.b > 0.8 && color.a == 1);
    }

    public static void PlaySound(PlayerSettings settings, AudioClip sound, Vector3 position, float volume)
    {
        if(settings.masterVolumeSlider.value > 0)
        {
            AudioSource.PlayClipAtPoint(sound, position, volume);
        }
    }

    public static Color generateBlobColor()
    {
        return Random.ColorHSV(0f, 1f, 0.8f, 0.86f, 1f, 1f, 1f, 1f);
    }

    public static Color generateFoodColor()
    {
        return Random.ColorHSV(0f, 1f, 1f, 1f, 0.94f, 1f, 1f, 1f);
    }
    
    public static Vector2 GenerateRandomDirection()
    {
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    public static bool CanEat(Transform one, Transform two)
    {
        if(one.localScale.x > two.localScale.x * 1.15f)
        {
            return true;
        }

        return false;
    }
    
    public static string GenerateRandomAlphanumericString(int length = 10)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    
        var random = new System.Random();
        var randomString = new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        return randomString;
    }

    public static int GetNeededExperience(int level)
    {
        return (level * level * 700) + 1000;
    }
}
