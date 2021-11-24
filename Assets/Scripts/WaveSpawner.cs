using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class WaveSpawner : MonoBehaviour
{
    public enum SpawnState {
        SPAWNING,
        WAITING,
        COUNTING
    };

    [System.Serializable]
    public class Wave
    {
        public string name;
        public Transform[] enemies;
        public int count;
        public float rate;
    }

    public Wave[] waves;
    public float timeBetweenWaves = 5f;
    public Transform[] spawnPoints;
    public GameObject RedWalls;
    public GameObject GreenWalls;

    private float waveCountdown;
    private SpawnState state = SpawnState.COUNTING;
    private int nextWave = 0;
    private float enemyCheckCountdown = 1f;

    private void Start()
    {
        waveCountdown = timeBetweenWaves;
    }

    private void Update()
    {
        if(state == SpawnState.COUNTING)
        {
            GameManager.Instance.ShowWaveName();
            GameManager.Instance.ShowCountdown();
        }
        else
        {
            GameManager.Instance.HideWaveName();
            GameManager.Instance.HideCountdown();
        }

        if (state == SpawnState.WAITING)
        {
            if (IsEnemyAlive())
            {
                return;
            }

            // New Wave
            state = SpawnState.COUNTING;
            GreenWalls.SetActive(true);
            RedWalls.SetActive(false);
            waveCountdown = timeBetweenWaves;
            
            if (nextWave + 1 == waves.Length)
            {
                GameManager.Instance.WinGame();
                gameObject.SetActive(false);
            }
            else
            {
                nextWave++;
            }
            
        }

        if (waveCountdown <= 0)
        {
            GreenWalls.SetActive(false);
            RedWalls.SetActive(true);
            GameManager.Instance.HideWaveName();
            GameManager.Instance.HideCountdown();
            if (state != SpawnState.SPAWNING)
            {
                StartCoroutine(SpawnWave(waves[nextWave]));
            }
        }
        else
        {
            int countDownInt = Mathf.FloorToInt(waveCountdown) + 1;
            GameManager.Instance.SetWaveNameText(waves[nextWave].name);
            GameManager.Instance.SetCountdownText(countDownInt.ToString());
            waveCountdown -= Time.deltaTime;
        }
    }

    bool IsEnemyAlive()
    {
        enemyCheckCountdown -= Time.deltaTime;
        if (enemyCheckCountdown <= 0)
        {
            enemyCheckCountdown = 1f;
            return GameObject.FindGameObjectsWithTag("Enemy").Length != 0;
        }

        return true;
    }

    IEnumerator SpawnWave(Wave wave)
    {
        state = SpawnState.SPAWNING;

        for(int i = 0; i < wave.count; i++)
        {
            SpawnEnemy(wave.enemies[Random.Range(0, wave.enemies.Length)]);
            yield return new WaitForSeconds(1f / wave.rate);
        }

        state = SpawnState.WAITING;
    }

    void SpawnEnemy(Transform enemy)
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(enemy, spawnPoint.position, Quaternion.identity);
    }
}
