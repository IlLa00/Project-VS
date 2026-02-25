using UnityEngine;

namespace VS.Core
{
    public class InfiniteBackground : MonoBehaviour
    {
        [SerializeField] private Sprite[] tileSprites;  
        [SerializeField] private int poolCountX = 22;
        [SerializeField] private int poolCountY = 22;
        [SerializeField] private int sortingOrder = -10;

        private Transform _cam;
        private Transform[] _tiles;
        private float _tileSize;
        private float _gridW, _gridH;

        void Start()
        {
            _cam = Camera.main.transform;

            _tileSize = tileSprites[0].bounds.size.x;
            _gridW = poolCountX * _tileSize;
            _gridH = poolCountY * _tileSize;

            _tiles = new Transform[poolCountX * poolCountY];

            for (int y = 0; y < poolCountY; y++)
            {
                for (int x = 0; x < poolCountX; x++)
                {
                    var go = new GameObject($"BG_{x}_{y}");
                    go.transform.SetParent(transform);

                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = tileSprites[Random.Range(0, tileSprites.Length)];
                    sr.sortingOrder = sortingOrder;

                    go.transform.position = new Vector3(
                        _cam.position.x + (x - poolCountX * 0.5f) * _tileSize,
                        _cam.position.y + (y - poolCountY * 0.5f) * _tileSize,
                        0f
                    );

                    _tiles[y * poolCountX + x] = go.transform;
                }
            }
        }

        void LateUpdate()
        {
            if (_cam == null) 
                return;

            float halfW = _gridW * 0.5f;
            float halfH = _gridH * 0.5f;
            float camX = _cam.position.x;
            float camY = _cam.position.y;

            foreach (var tile in _tiles)
            {
                float px = tile.position.x;
                float py = tile.position.y;

                float dx = camX - px;
                if (dx > halfW)       
                    px += _gridW;
                else if (dx < -halfW) 
                    px -= _gridW;

                float dy = camY - py;
                if (dy > halfH)       
                    py += _gridH;
                else if (dy < -halfH) 
                    py -= _gridH;

                tile.position = new Vector3(px, py, 0f);
            }
        }
    }
}
