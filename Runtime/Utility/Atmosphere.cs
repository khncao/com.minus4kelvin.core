using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Characters;

namespace m4k {
[System.Serializable]
public class ProceduralSkyboxProfile {
    public Color sunTint;
    public float sunStrength;
    public float hdrExposure;
}

public class Atmosphere : MonoBehaviour
{
    [System.Serializable]
    public class WeatherProbability {
        public ParticleSystem ps;
        [Range(0, 1f)]
        public float hourlyProb;
        [HideInInspector]
        public bool isActive;
    }
    public Transform celestialRotator;
    public Light sun, moon;
    public ProceduralSkyboxProfile daySky, nightSky;
    public float toDayTime = 10f, toNightTime = 10f;
    public WeatherProbability rain, snow, clouds, fog, wind, thunder;
    // public Light lightning;
    // [Range(0, 1f)]
    // public float lightningDuration = 0.5f, lightningMinInterval = 0.1f, lightningChance = 0.5f;
    // public float clusterMinInterval = 5;
    // public int clusterMaxSize = 4;
    // public AudioSource rainAudio, windAudio;
    // public AudioClip lightRainAudioClip, heavyRainAudioClip, lightWindAudioClip, strongWindAudioClip;
    [Min(10)]
    public int durationRandRange;
    // [Min(5)]
    // public int transitionRandRange;
    WeatherProbability currPrecip;
    bool precipActive { get { return rain.isActive || snow.isActive; }}
    float lightningInitIntens;
    float initSunIntens;
    float initMoonIntens;
    ProceduralSkyboxProfile initDaySky, initNightSky;
    Material skybox;
    // int precipEndTime;
    bool prevDayState, isDay;

    private void Awake() {
        // lightningInitIntens = lightning.intensity;
        initSunIntens = sun.intensity;
        initMoonIntens = moon.intensity;
        // initDaySky.hdrExposure = Renders
        RenderSettings.skybox = Instantiate(RenderSettings.skybox);
        skybox = RenderSettings.skybox;
    }

    private void Start() {
        // Game.I.hourly += Hourly;
        // Game.I.monthly += Monthly;
        // Monthly(6);

        // var rate = (Game.time / 1440) * 360;
        // isDay = rate > 80 && rate < 270;
        // prevDayState = !isDay;
        // if(isDay)
        //     SetDaytime(true);
        // else
        //     SetNighttime(true);
    }
    // private void OnDisable() {
    //     if(!Game.I) return;
    //     Game.I.hourly -= Hourly;
    //     Game.I.monthly -= Monthly;
    // }
    public void Tick(float time) {
        if(CharacterManager.I.Player)
            transform.position = CharacterManager.I.Player.transform.position;
        // 24*60=1440, 1440*60=86400
        var rate = (time / 1440) * 360;
        var euler = celestialRotator.eulerAngles;
        // Vector3 newEuler = new Vector3(euler.x, euler.y, rate);
        // newEuler = Vector3.SmoothDamp(euler, newEuler, ref vel, 0.1f);
        celestialRotator.eulerAngles = new Vector3(euler.x, euler.y, rate);

        //6:00=90, 18:00=270
        isDay = rate > 80 && rate < 270;
        if(isDay != prevDayState) {
            if(isDay)
                SetDaytime();
            else
                SetNighttime();

            prevDayState = isDay;
        }
    }
    public void Hourly(int time) {
        if(!precipActive && currPrecip != null) {
            ProcessWeatherProbability(currPrecip);
        }
        if(clouds != null)
            ProcessWeatherProbability(clouds);
        if(fog != null)
            ProcessWeatherProbability(fog);
        if(thunder != null)
            ProcessWeatherProbability(thunder);
        // if(wind != null)
        //     ProcessWeatherProbability(wind);
    }
    public void Monthly(int month) {
        if(month > 11 || month < 2) currPrecip = snow;
        else currPrecip = rain;
    }
    IEnumerator StartEndWeatherTimer(WeatherProbability weather, float intensity, float timer) {
        weather.ps.Play();
        weather.isActive = true;
        // if(weather == rain) {
        //     if(Random.Range(0, 2) == 1)
        //         rainAudio.PlayOneShot(heavyRainAudioClip);
        //     else
        //         rainAudio.PlayOneShot(lightRainAudioClip);
        // }
        // else if(weather == wind) {
        //     if(Random.Range(0, 2) == 1)
        //         windAudio.PlayOneShot(strongWindAudioClip);
        //     else
        //         windAudio.PlayOneShot(lightWindAudioClip);
        // }
        yield return new WaitForSeconds(timer);
        weather.ps.Stop();
        weather.isActive = false;
        // if(weather == rain) rainAudio.Stop();
        // else if(weather == wind) windAudio.Stop();
    }
    void ProcessWeatherProbability(WeatherProbability weather) {
        if(!weather.isActive) {
            // if((weather == rain || weather == snow) && precipActive) 
            //     return;
            if(Random.Range(0, 1f) < weather.hourlyProb) {
                float intensity = Random.Range(0.5f, 1f);
                // if(weather == thunder)
                //     StartCoroutine(ThunderLightning(intensity, Random.Range(10, durationRandRange)));
                if(weather.ps)
                    StartCoroutine(StartEndWeatherTimer(weather, intensity, Random.Range(10, durationRandRange)));
            }
        }
    }

    public void SetDaytime(bool instant = false) {
        if(transitionToNightCR != null)
            StopCoroutine(transitionToNightCR);
        if(instant) {
            sun.intensity = initSunIntens;
            sun.enabled = true;
            moon.enabled = false;
            skybox.SetFloat("_HdrExposure", daySky.hdrExposure);
            skybox.SetColor("_SunTint", daySky.sunTint);
            skybox.SetFloat("_SunStrength", daySky.sunStrength);
            DynamicGI.UpdateEnvironment();
        }
        else if(transitionToDayCR == null)
            StartCoroutine(TransitionToDay(toDayTime));
    }
    public void SetNighttime(bool instant = false) {
        if(transitionToDayCR != null)
            StopCoroutine(transitionToDayCR);
        if(instant) {
            moon.intensity = initMoonIntens;
            moon.enabled = true;
            moon.enabled = false;
            skybox.SetFloat("_HdrExposure", nightSky.hdrExposure);
            skybox.SetColor("_SunTint", nightSky.sunTint);
            skybox.SetFloat("_SunStrength", nightSky.sunStrength);
            DynamicGI.UpdateEnvironment();
        }
        else if(transitionToNightCR == null)
            StartCoroutine(TransitionToNight(toNightTime));
    }

    // Coroutine smoothDisableLightCR, smoothEnableLightCR;
    Coroutine transitionToDayCR, transitionToNightCR;
    IEnumerator TransitionToDay(float timer) {
        float time = timer;
        sun.enabled = true;
        while(timer > 0f) {
            timer -= Time.deltaTime;
            var rate = timer/time;
            sun.intensity = (1 - rate) * initSunIntens;
            moon.intensity = rate * initMoonIntens;

            skybox.SetFloat("_HdrExposure", Mathf.Lerp(daySky.hdrExposure, nightSky.hdrExposure, rate));
            // skybox.SetColor("_SunTint", Color.Lerp(daySky.sunTint, nightSky.sunTint, rate));
            // skybox.SetFloat("_SunStrength", Mathf.Lerp(daySky.sunStrength, nightSky.sunStrength, rate));
            if(rate > 0.45f && rate < 0.55f) {
                skybox.SetColor("_SunTint", daySky.sunTint);
                skybox.SetFloat("_SunStrength", daySky.sunStrength);
            }

            yield return null;
        }
        moon.enabled = false;
        DynamicGI.UpdateEnvironment();
        yield return null;
    }

    IEnumerator TransitionToNight(float timer) {
        float time = timer;
        moon.enabled = true;
        while(timer > 0f) {
            timer -= Time.deltaTime;
            var rate = timer/time;
            sun.intensity = rate * initSunIntens;
            moon.intensity = (1 - rate) * initMoonIntens;

            skybox.SetFloat("_HdrExposure", Mathf.Lerp(nightSky.hdrExposure, daySky.hdrExposure, rate));
            // skybox.SetColor("_SunTint", Color.Lerp(nightSky.sunTint, daySky.sunTint, rate));
            // skybox.SetFloat("_SunStrength", Mathf.Lerp(nightSky.sunStrength, daySky.sunStrength, rate));
            if(rate > 0.45f && rate < 0.55f) {
                skybox.SetColor("_SunTint", nightSky.sunTint);
                skybox.SetFloat("_SunStrength", nightSky.sunStrength);
            }
            
            yield return null;
        }
        sun.enabled = false;
        DynamicGI.UpdateEnvironment();
        yield return null;
    }

    bool AreAllDirLightsDisabled() {
        if(sun.enabled || moon.enabled)
            return false;
        return true;
    }
    // Coroutine lightningFlashCR;
    // IEnumerator ThunderLightning(float initIntens, float totalDur) {
    //     float initTotalDur = totalDur;
    //     float lastFlash = 0;
    //     initIntens *= lightningInitIntens;

    //     while(totalDur > 0) {
    //         totalDur -= Time.deltaTime;
    //         lightning.enabled = true;
    //         lightning.intensity = initIntens;
    //         float flashTimer = 0;

    //         while(flashTimer < lightningDuration) {
    //             flashTimer += Time.deltaTime;
    //             lightning.intensity = totalDur / initTotalDur * initIntens;
    //         }
    //         lightning.enabled = false;

    //         yield return null;
    //     }
    // }

    // IEnumerator WeatherTransitionIn(float timer) {
    //     float initTime = timer;
    //     while(timer > 0) {
    //         float rate = timer / initTime;
    //         var emmiss = currPrecip.ps.emission;
    //         emmiss.rateOverTime = 
    //         currPrecip.ps. = emmiss;
    //         timer -= Time.deltaTime;
    //         yield return null;
    //     }
    // }
    // IEnumerator WeatherTransitionOut(float timer) {

    // }
}
}