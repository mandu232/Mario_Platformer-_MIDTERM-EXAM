using UnityEngine;
using UnityEngine.Tilemaps;

namespace Midterm
{
    [RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]

    
    public sealed class BreakableBlock : MonoBehaviour
    {

        
        [SerializeField] BlockBreakEffect breakEffectPrefab;


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


            var effect = Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(effect.gameObject, gameObject.scene);
            effect.Play(visual);

            if (sourceMap != null) 
            {
                sourceMap.SetTile(sourceCell, null);
            }



            Destroy(gameObject);
        }
        //----------
    }
}
