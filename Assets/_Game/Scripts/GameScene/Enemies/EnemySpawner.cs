using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemySpawner : MonoBehaviour
{
    [field: SerializeField] public UnityEvent<EnemyBehavior> OnEnemySpawn;

    [SerializeField] private EnemyBehavior _enemyPrefab;
    private Vector2 _sceneSize;

    private Camera cam;

    IEnumerator IdleSpawn() {
        while(true) { 
            SpawnEnemy(Vector2.up * _sceneSize.y + Vector2.right * Random.Range(_sceneSize.x, -_sceneSize.x));
            yield return new WaitForSeconds(0.5f);
            SpawnEnemy(Vector2.down * _sceneSize.y + Vector2.right * Random.Range(_sceneSize.x, -_sceneSize.x));
            yield return new WaitForSeconds(0.5f);
            SpawnEnemy(Vector2.left * _sceneSize.x + Vector2.up * Random.Range(_sceneSize.y, -_sceneSize.y));
            yield return new WaitForSeconds(0.5f);
            SpawnEnemy(Vector2.right * _sceneSize.x + Vector2.up * Random.Range(_sceneSize.y, -_sceneSize.y));
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator BurstSpawn() {
        while(true) {
            yield return new WaitForSeconds(2f);
            for(int i = 0; i < 3; i++) { 
                for(int j = 0; j < 5; j++) {
                    SpawnEnemy(Vector2.up * _sceneSize.y + Vector2.right * Random.Range(_sceneSize.x, -_sceneSize.x));
                }
                yield return new WaitForSeconds(0.2f);
            }
            
            yield return new WaitForSeconds(15f);
            for(int i = 0; i < 3; i++) {
                for(int j = 0; j < 5; j++) {
                    SpawnEnemy(Vector2.down * _sceneSize.y + Vector2.right * Random.Range(_sceneSize.x, -_sceneSize.x));
                }
                yield return new WaitForSeconds(0.2f);
            }
            
            yield return new WaitForSeconds(15f);
            for(int i = 0; i < 3; i++) {
                for(int j = 0; j < 5; j++) {
                    SpawnEnemy(Vector2.left * _sceneSize.x + Vector2.up * Random.Range(_sceneSize.y, -_sceneSize.y));
                }
                yield return new WaitForSeconds(0.2f);
            }
            
            yield return new WaitForSeconds(15f);
            for(int i = 0; i < 3; i++) {
                for(int j = 0; j < 5; j++) {
                    SpawnEnemy(Vector2.right * _sceneSize.x + Vector2.up * Random.Range(_sceneSize.y, -_sceneSize.y));
                }
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    private void Awake() {
        cam = Camera.main;
        UpdateScreenSize();

        StartCoroutine(IdleSpawn());
        StartCoroutine(BurstSpawn());
    }

    private void Update() {
        UpdateScreenSize();
    }

    private void UpdateScreenSize() {
        _sceneSize.y = cam.orthographicSize * 2f * 1.1f;
        _sceneSize.x = _sceneSize.y * cam.aspect;
        _sceneSize /= 2;
    } 

    private void SpawnEnemy(Vector2 position) {
        EnemyBehavior enemy = Instantiate(_enemyPrefab, position, Quaternion.identity, null);
        OnEnemySpawn.Invoke(enemy);
    }
}
