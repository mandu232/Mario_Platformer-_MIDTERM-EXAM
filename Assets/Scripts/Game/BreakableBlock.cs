using UnityEngine;
using UnityEngine.Tilemaps;

namespace Midterm
{
    [RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]

    
    public sealed class BreakableBlock : MonoBehaviour
    {

        
        [SerializeField] Sprite[] fragmentSprites;


        Tilemap sourceMap;
        Vector3Int sourceCell;
        bool broken;

        public void BindCell(Tilemap map, Vector3Int cell)
        {
            sourceMap = map;
            sourceCell = cell;
        }


        void OnCollisionEnter2D(Collision2D collision)
        {
            if (broken || collision.gameObject.GetComponent<PlayerController2D>() == null)
            {
                return;
            }

            if (collision.transform.position.y >= transform.position.y || collision.relativeVelocity.y <= 0.1f)
            {
                return;
            }

            for (int i = 0; i < collision.contactCount; i++)
            {
                if (collision.GetContact(i).normal.y > 0.5f)
                {
                    Break();
                    return;
                }
            }
        }




        //AiCode------
        void Break()
        {
            broken = true;
            GameAudio.Play(GameSound.BlockBreak);

            GetComponent<Collider2D>().enabled = false;

            var visual = GetComponent<SpriteRenderer>();


            //파괴시 파편생성
            for (int i = 0; i < 4; i++)
            {
                var piece = new GameObject("BlockFragment", typeof(SpriteRenderer), typeof(Rigidbody2D));
                piece.transform.position = transform.position + new Vector3(i % 2 == 0 ? -0.15f : 0.15f, i < 2 ? -0.15f : 0.15f, 0);
                var sprite = piece.GetComponent<SpriteRenderer>();



                sprite.sprite = fragmentSprites[i % fragmentSprites.Length];
                sprite.sharedMaterial = visual.sharedMaterial;
                sprite.sortingLayerID = visual.sortingLayerID;
                sprite.sortingOrder = visual.sortingOrder + 1;



                var body = piece.GetComponent<Rigidbody2D>();
                float direction = i % 2 == 0 ? -1 : 1;



                body.gravityScale = 2;
                body.linearVelocity = new Vector2(direction * (i < 2 ? 2.5f : 4f), i < 2 ? 4f : 6f);
                body.angularVelocity = direction * 300f;

                Destroy(piece, 2f);
            }



            if (sourceMap != null) 
            {
                sourceMap.SetTile(sourceCell, null);
            }



            Destroy(gameObject);
        }
        //----------
    }
}
