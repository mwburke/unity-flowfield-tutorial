using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class noiseFlowfield : MonoBehaviour

{
    FastNoise _fastNoise;
    public Vector3Int _gridSize;
    public float _cellSize;
    public Vector3[,,] _flowfieldDirection;
    public float _increment;
    public Vector3 _offset, _offsetSpeed;

    // particles (well, really spheres)
    public GameObject _particlePrefab;
    public int _amountOfParticles;
    [HideInInspector]
    public List<flowFieldParticle> _particles;
    public float _particleScale, _particleMoveSpeed, _particleRotateSpeed;

    // Start is called before the first frame update
    void Start()
    {
        _flowfieldDirection = new Vector3[_gridSize.x, _gridSize.y, _gridSize.z];
        _fastNoise = new FastNoise();
        _particles = new List<flowFieldParticle>();
        for (int i = 0; i < _amountOfParticles; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(this.transform.position.x, this.transform.position.x + _gridSize.x * _cellSize),
                Random.Range(this.transform.position.y, this.transform.position.y + _gridSize.y * _cellSize),
                Random.Range(this.transform.position.z, this.transform.position.z + _gridSize.z * _cellSize)
            );

            GameObject particleInstance = (GameObject)Instantiate(_particlePrefab);
            particleInstance.transform.position = randomPos;
            particleInstance.transform.parent = this.transform;
            particleInstance.transform.localScale = new Vector3(_particleScale, _particleScale, _particleScale);
            _particles.Add(particleInstance.GetComponent<flowFieldParticle>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        CalculateFlowfieldDirections();
        ParticleBehavior();
    }

    void CalculateFlowfieldDirections()
    {
        _offset = new Vector3(_offset.x + (_offsetSpeed.x * Time.deltaTime), _offset.y + (_offsetSpeed.y * Time.deltaTime), _offset.z + (_offsetSpeed.z * Time.deltaTime));

        float xOff = 0f;
        for (int x = 0; x < _gridSize.x; x++)
        {
            float yOff = 0f;
            for (int y = 0; y < _gridSize.y; y++)
            {
                float zOff = 0f;
                for (int z = 0; z < _gridSize.z; z++)
                {
                    float noise = (_fastNoise.GetSimplex(xOff + _offset.x, yOff + _offset.y, zOff + _offset.z) + 1f); // We add 1 because it goes from -1 to 1
                    Vector3 noiseDirection = new Vector3(Mathf.Cos(noise * Mathf.PI), Mathf.Sin(noise * Mathf.PI), Mathf.Cos(noise * Mathf.PI));
                    _flowfieldDirection[x, y, z] = Vector3.Normalize(noiseDirection);
                    zOff += _increment;
                }

                yOff += _increment;
            }
            xOff += _increment;
        }
    }

    void ParticleBehavior()
    {
        foreach(flowFieldParticle p in _particles)
        {
            Debug.Log(p.transform.position);
            // ugly boundary checking and reassignment instead of just using a mod function
            // x
            if (p.transform.position.x > this.transform.position.x + (_gridSize.x * _cellSize))
            {
                p.transform.position = new Vector3(this.transform.position.x, p.transform.position.y, p.transform.position.z);
            }
            if (p.transform.position.x < this.transform.position.x)
            {
                p.transform.position = new Vector3(this.transform.position.x + (_gridSize.x * _cellSize), p.transform.position.y, p.transform.position.z);
            }
            // y
            if (p.transform.position.y > this.transform.position.y + (_gridSize.y * _cellSize))
            {
                p.transform.position = new Vector3(p.transform.position.x, this.transform.position.y, p.transform.position.z);
            }
            if (p.transform.position.y < this.transform.position.y)
            {
                p.transform.position = new Vector3(p.transform.position.x, this.transform.position.y + (_gridSize.y * _cellSize), p.transform.position.z);
            }
            // z
            if (p.transform.position.z > this.transform.position.z + (_gridSize.z * _cellSize))
            {
                p.transform.position = new Vector3(p.transform.position.x, p.transform.position.y, this.transform.position.z);
            }
            if (p.transform.position.z < this.transform.position.z)
            {
                p.transform.position = new Vector3(p.transform.position.x, p.transform.position.y, this.transform.position.z + (_gridSize.z * _cellSize));
            }

            Vector3Int particlePos = new Vector3Int(
                Mathf.FloorToInt(Mathf.Clamp((p.transform.position.x - this.transform.position.x) / _cellSize, 0, _gridSize.x - 1)),
                Mathf.FloorToInt(Mathf.Clamp((p.transform.position.y - this.transform.position.y) / _cellSize, 0, _gridSize.y - 1)),
                Mathf.FloorToInt(Mathf.Clamp((p.transform.position.z - this.transform.position.z) / _cellSize, 0, _gridSize.z - 1))
            );

            p.ApplyRotation(_flowfieldDirection[particlePos.x, particlePos.y, particlePos.z], _particleRotateSpeed);
            p._moveSpeed = _particleMoveSpeed;
            p.transform.localScale = new Vector3(_particleScale, _particleScale, _particleScale);
        }
    }

    private void OnDrawGizmos()
    {
         Gizmos.color = Color.white;
         Gizmos.DrawWireCube(this.transform.position + new Vector3(_gridSize.x * _cellSize / 2f, _gridSize.y * _cellSize / 2f, _gridSize.z * _cellSize / 2f),
            new Vector3(_gridSize.x * _cellSize, _gridSize.y * _cellSize, _gridSize.y * _cellSize));

/*        _fastNoise = new FastNoise();

        float xOff = 0f;
        for (int x = 0; x < _gridSize.x; x++)
        {
            float yOff = 0f;
            for (int y = 0; y < _gridSize.y; y++)
            {
                float zOff = 0f;
                for (int z = 0; z < _gridSize.z; z++)
                {
                    float noise = (_fastNoise.GetSimplex(xOff + _offset.x, yOff + _offset.y, zOff + _offset.z) + 1f) / 2; // We add 1 because it goes from -1 to 1
                    Vector3 noiseDirection = new Vector3(Mathf.Cos(noise * Mathf.PI), Mathf.Sin(noise * Mathf.PI), Mathf.Cos(noise * Mathf.PI));
                    Vector3 pos = new Vector3(x, y, z) + transform.position;
                    Vector3 endpos = pos + Vector3.Normalize(noiseDirection);
                    Gizmos.color = new Color(noiseDirection.normalized.x, noiseDirection.normalized.y, noiseDirection.normalized.z, 0.4f);
                    Gizmos.DrawLine(pos, endpos);
                    Gizmos.DrawSphere(endpos, 0.1f);
                    zOff += +_increment;
                }

                yOff += _increment;
            }
            xOff += _increment;
        }*/
    }
}
