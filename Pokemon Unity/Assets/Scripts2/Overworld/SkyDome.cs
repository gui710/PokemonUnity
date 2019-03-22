﻿using System;
using UnityEngine;

namespace PokemonUnity.Overworld
{
public class SkyDome
{
    private Model SkydomeModel;
    public Texture2D TextureUp;
    public Texture2D TextureDown;
    private Texture2D TextureSun;
    private Texture2D TextureMoon;

    public float Yaw = 0;

    const bool FASTTIMECYCLE = false;

    public SkyDome()
    {
        SkydomeModel = Core.Content.Load<Model>(@"SkyDomeResource\SkyDome");

        TextureUp = TextureManager.GetTexture(@"SkyDomeResource\Clouds");
        TextureDown = TextureManager.GetTexture(@"SkyDomeResource\Clouds");
        TextureSun = TextureManager.GetTexture(@"SkyDomeResource\sun");
        TextureMoon = TextureManager.GetTexture(@"SkyDomeResource\moon");

        SetLastColor();
    }

    public void Update()
    {
        Yaw += 0.0002F;
        while (Yaw > MathHelper.TwoPi)
            Yaw -= MathHelper.TwoPi;
        SetLastColor();

        if (FASTTIMECYCLE == true)
        {
            Second += 60;
            if (Second == 60)
            {
                Second = 0;
                Minute += 1;
                if (Minute == 60)
                {
                    Minute = 0;
                    Hour += 1;
                    if (Hour == 24)
                        Hour = 0;
                }
            }
        }
    }

    private int Hour = 0;
    private int Minute = 0;
    private int Second = 0;

    private float GetUniversePitch()
    {
        if (FASTTIMECYCLE == true)
        {
            int progress = Hour * 3600 + Minute * 60 + Second;
            return System.Convert.ToSingle((MathHelper.TwoPi / (double)100) * (progress / (double)86400 * 100));
        }
        else
        {
            int progress = World.SecondsOfDay;
            return System.Convert.ToSingle((MathHelper.TwoPi / (double)100) * (progress / (double)86400 * 100));
        }
    }

    public void Draw(float FOV)
    {
        if (Core.GameOptions.GraphicStyle == 1)
        {
            if (Screen.Level.World.EnvironmentType == World.EnvironmentTypes.Outside)
            {
                if (World.GetWeatherFromWeatherType(Screen.Level.WeatherType) != World.Weathers.Fog)
                {
                    RenderHalf(FOV, MathHelper.PiOver2, System.Convert.ToSingle(GetUniversePitch() + Math.PI), true, TextureSun, 100, this.GetSunAlpha()); // Draw the Sun.
                    RenderHalf(FOV, MathHelper.PiOver2, System.Convert.ToSingle(GetUniversePitch()), true, TextureMoon, 100, GetStarsAlpha()); // Draw the Moon.
                    RenderHalf(FOV, MathHelper.PiOver2, System.Convert.ToSingle(GetUniversePitch()), true, TextureDown, 50, GetStarsAlpha()); // Draw the first half of the stars.
                    RenderHalf(FOV, MathHelper.PiOver2, System.Convert.ToSingle(GetUniversePitch()), false, TextureDown, 50, GetStarsAlpha()); // Draw the second half of the stars.
                    RenderHalf(FOV, MathHelper.TwoPi - Yaw, 0.0F, true, GetCloudsTexture(), 15, GetCloudAlpha()); // Draw the back layer of the clouds.
                    RenderHalf(FOV, Yaw, 0.0F, true, TextureUp, 10, GetCloudAlpha()); // Draw the front layer of the clouds.
                }
            }
            else
            {
                RenderHalf(FOV, Yaw, 0.0F, true, TextureUp, 8.0F, 1.0F);

                if (TextureDown != null)
                    RenderHalf(FOV, Yaw, 0.0F, false, TextureDown, 8.0F, 1.0F);
            }
        }
    }

    private void RenderHalf(float FOV, float useYaw, float usePitch, bool up, Texture2D texture, float scale, float alpha)
    {
        float Roll = 0.0F;
        if (up == false)
            Roll = (float)Math.PI;

        var previousBlendState = Core.GraphicsDevice.BlendState;
        Core.GraphicsDevice.BlendState = BlendState.NonPremultiplied;

        foreach (ModelMesh ModelMesh in SkydomeModel.Meshes)
        {
            foreach (BasicEffect BasicEffect in ModelMesh.Effects)
            {
                BasicEffect.World = Matrix.CreateScale(scale) * Matrix.CreateTranslation(new Vector3(Screen.Camera.Position.X, -5, Screen.Camera.Position.Z)) * Matrix.CreateFromYawPitchRoll(useYaw, usePitch, Roll);

                BasicEffect.View = Screen.Camera.View;
                BasicEffect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FOV), Core.GraphicsDevice.Viewport.AspectRatio, 0.01, 10000);

                BasicEffect.TextureEnabled = true;
                BasicEffect.Texture = texture;
                BasicEffect.Alpha = alpha;

                switch (Screen.Level.World.CurrentMapWeather)
                {
                    case World.Weathers.Clear:
                    case World.Weathers.Sunny:
                        {
                            BasicEffect.DiffuseColor = new Vector3(1f, 1f, 1f);
                            break;
                        }
                    case World.Weathers.Rain:
                        {
                            BasicEffect.DiffuseColor = new Vector3(0.4f, 0.4f, 0.7f);
                            break;
                        }
                    case World.Weathers.Snow:
                        {
                            BasicEffect.DiffuseColor = new Vector3(0.8f, .8f, .8f);
                            break;
                        }
                    case World.Weathers.Underwater:
                        {
                            BasicEffect.DiffuseColor = new Vector3(0.1f, 0.3f, 0.9f);
                            break;
                        }
                    case World.Weathers.Fog:
                        {
                            BasicEffect.DiffuseColor = new Vector3(0.7f, 0.7f, 0.8f);
                            break;
                        }
                    case World.Weathers.Sandstorm:
                        {
                            BasicEffect.DiffuseColor = new Vector3(0.8f, 0.5f, 0.2f);
                            break;
                        }
                    case World.Weathers.Ash:
                        {
                            BasicEffect.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
                            break;
                        }
                    case World.Weathers.Blizzard:
                        {
                            BasicEffect.DiffuseColor = new Vector3(0.6f, 0.6f, 0.6f);
                            break;
                        }
                }

                if (BasicEffect.DiffuseColor != new Vector3(1f, 1f, 1f))
                    BasicEffect.DiffuseColor = GetWeatherColorMultiplier(BasicEffect.DiffuseColor);
            }

            ModelMesh.Draw();
        }

        Core.GraphicsDevice.BlendState = previousBlendState;
    }

    private static Color[] DaycycleTextureData = null;
    private static Texture2D DaycycleTexture = null/* TODO Change to default(_) if this is not a reference type */;
    private static Color LastSkyColor = new Color(0, 0, 0, 0);
    private static Color LastEntityColor = new Color(0, 0, 0, 0);

    public static Color GetDaytimeColor(bool shader)
    {
        if (shader == true)
            return LastEntityColor;
        else
            return LastSkyColor;
    }

    private void SetLastColor()
    {
        if (DaycycleTextureData == null)
        {
            Texture2D DaycycleTexture = TextureManager.GetTexture(@"SkyDomeResource\daycycle");
            DaycycleTextureData = new Color[DaycycleTexture.Width * DaycycleTexture.Height - 1 + 1];
            DaycycleTexture.GetData(DaycycleTextureData);
            SkyDome.DaycycleTexture = DaycycleTexture;
        }

        int pixel = GetTimeValue();

        Color pixelColor = DaycycleTextureData[pixel];
        if (pixelColor != LastSkyColor)
        {
            LastSkyColor = pixelColor;
            LastEntityColor = DaycycleTextureData[(pixel + DaycycleTexture.Width).Clamp(0, DaycycleTexture.Width * DaycycleTexture.Height - 1)];
        }
    }

    private float GetCloudAlpha()
    {
        switch (Screen.Level.World.CurrentMapWeather)
        {
            case World.Weathers.Rain:
            case World.Weathers.Blizzard:
            case World.Weathers.Thunderstorm:
                {
                    return 0.6F;
                }
            case World.Weathers.Snow:
            case World.Weathers.Ash:
                {
                    return 0.4F;
                }
            case World.Weathers.Clear:
                {
                    return 0.1F;
                }
        }
        return 0.0F;
    }

    private float GetStarsAlpha()
    {
        int progress = GetTimeValue();

        if (progress < 360 | progress > 1080)
        {
            int dP = progress;
            if (dP < 360)
                dP = 720 - dP * 2;
            else if (dP > 1080)
                dP = 720 - (1440 - dP) * 2;

            float alpha = System.Convert.ToSingle(dP / (double)720) * 0.7F;
            return alpha;
        }
        else
            return 0.0F;
    }

    private float GetSunAlpha()
    {
        int progress = GetTimeValue();

        if (progress >= 1080 & progress < 1140)
        {
            // Between 6:00:00 PM and 7:00:00 PM, the Sun will fade away with 60 stages:
            float i = progress - 1080;
            float percent = i / (float)60 * 100f;

            return 1.0F - percent / (float)100.0F;
        }
        else if (progress >= 300 & progress < 360)
        {
            // Between 5:00:00 AM and 6:00:00 Am, the Sun will fade in with 60 stages:
            float i = progress - 300;
            float percent = i / (float)60 * 100;

            return percent / (float)100.0F;
        }
        else if (progress >= 1140 | progress < 300)
            // Between 7:00:00 PM and 5:00:00 AM, the Sun will be invisible:
            return 0.0F;
        else
            // Between 6:00:00 AM and 6:00:00 PM, the Sun will be fully visible:
            return 1.0F;
    }

    private Texture2D GetCloudsTexture()
    {
        switch (Screen.Level.World.CurrentMapWeather)
        {
            case World.Weathers.Rain:
            case World.Weathers.Blizzard:
            case World.Weathers.Thunderstorm:
            case World.Weathers.Snow:
                {
                    return TextureManager.GetTexture(@"SkyDomeResource\CloudsWeather");
                }
        }
        return TextureUp;
    }

    public Vector3 GetWeatherColorMultiplier(Vector3 v)
    {
        int progress = GetTimeValue();

        float p = 0.0F;

        if (progress < 720)
            p = System.Convert.ToSingle((720 - progress) / (double)720);
        else
            p = System.Convert.ToSingle((progress - 720) / (double)720);

        return new Vector3(v.X + ((1 - v.X) * p), v.Y + ((1 - v.Y) * p), v.Z + ((1 - v.Z) * p));
    }

    private int GetTimeValue()
    {
        if (FASTTIMECYCLE == true)
            return Hour * 60 + Minute;
        else
        {
            if (World.IsMainMenu)
                return 720;
            return World.MinutesOfDay;
        }
    }
}
}