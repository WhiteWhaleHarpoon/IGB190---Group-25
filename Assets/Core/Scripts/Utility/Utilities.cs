using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace MyUtilities
{
    public static class Utilities
    {
        public static T RandomItem<T>(this IEnumerable<T> input)
        {
            return input.ElementAt(Random.Range(0, input.Count()));
        }

        public static List<T> GetAllWithinRange<T>(Vector3 position, float maxDistance) where T : MonoBehaviour
        {
            List<T> withinRange = new List<T>();
            T[] objs = GameObject.FindObjectsByType<T>(FindObjectsSortMode.None);
            foreach (T obj in objs)
            {
                float distance = Vector3.Distance(obj.transform.position, position);
                if (distance < maxDistance)
                {
                    withinRange.Add(obj);
                }
            }
            return withinRange;
        }

        public static T GetClosest<T>(Vector3 position, float maxDistance) where T : MonoBehaviour
        {
            T closest = null;
            float closestDistance = maxDistance;
            T[] objs = GameObject.FindObjectsByType<T>(FindObjectsSortMode.None);
            foreach (T obj in objs)
            {
                float distance = Vector3.Distance(obj.transform.position, position);
                if (distance < closestDistance)
                {
                    closest = obj;
                    closestDistance = distance;
                }
            }
            return closest;
        }

        public static T GetFurthest<T>(Vector3 position, float maxDistance) where T : MonoBehaviour
        {
            T furthest = null;
            float furthestDistance = 0;
            T[] objs = GameObject.FindObjectsByType<T>(FindObjectsSortMode.None);
            foreach (T obj in objs)
            {
                float distance = Vector3.Distance(obj.transform.position, position);
                if (distance > furthestDistance)
                {
                    furthest = obj;
                    furthestDistance = distance;
                }
            }
            return furthest;
        }

        public static Vector3 GetMouseWorldPosition()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, GameManager.assets.floorMask))
            {
                return GetValidNavMeshPosition(hit.point);
            }
            
            else
            {
                
                Plane plane = new Plane(Vector3.up, GameManager.player.transform.position);
                float distance = 0;
                if (plane.Raycast(ray, out distance))
                {
                    return ray.GetPoint(distance);
                    //return GetValidNavMeshPosition(ray.GetPoint(distance));
                }
            }
            
            return Vector3.positiveInfinity;
        }

        public static Vector3 GetClosestPointInLOS (Vector3 start, Vector3 end)
        {
            start += Vector3.up * 0.1f;
            end += Vector3.up * 0.1f;
            if (Physics.Linecast(start, end, out RaycastHit hitInfo, GameManager.assets.wallMask))
            {
                end = hitInfo.point - (end - start).normalized * 0.5f;
            }
            end = GetValidNavMeshPosition(end);
            return end;
        }

        public static Vector3 GetClosestPointInLOS_OLD (Vector3 start)
        {
            Vector3 end;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, GameManager.assets.floorMask))
            {
                end = hit.point;
            }
            else
            {
                Plane plane = new Plane(Vector3.up, start);
                plane.Raycast(ray, out float distance);
                end = ray.GetPoint(distance);
                
            }
            if (Physics.Linecast(start, end, out RaycastHit hitInfo, GameManager.assets.wallMask))
            {
                end = hitInfo.point - (end - start).normalized * 0.5f;
            }
            end = GetValidNavMeshPosition(end);
            return end;
        }

        public static Interactable GetSelectedInteractable ()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, GameManager.assets.monsterMask))
            {
                return hit.collider.GetComponent<Interactable>();
            }
            return null;
        }

        public static Unit GetTarget()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                return hit.collider.GetComponent<Unit>();
            }
            return null;
        }

        public static float MapValue(float x, float in_min, float in_max, float out_min, float out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }

        /*
        public static List<Unit> FindUnitsInRange(Vector3 position, float range, Vector3 forward, float arcInDegrees = 360.0f)
        {
            List<Unit> results = new List<Unit>();
            Unit[] units = GameObject.FindObjectsByType<Unit>(FindObjectsSortMode.None);
            foreach (Unit unit in units)
            {
                if (Vector3.Distance(unit.transform.position, position) < range)
                {
                    //results.Add(unit);
                    float allowed = ((1.0f - arcInDegrees / 360.0f) - 0.5f) * 2.0f;
                    if (Vector3.Dot(forward, (unit.transform.position - position).normalized) >= allowed)
                    {
                        results.Add(unit);
                    }
                }
            }
            return results;
        }
        */

        public static Vector3 GetRandomPointInsideCollider(this BoxCollider boxCollider)
        {
            Vector3 extents = boxCollider.size / 2f;
            Vector3 point = new Vector3(
                Random.Range(-extents.x, extents.x),
                Random.Range(-extents.y, extents.y),
                Random.Range(-extents.z, extents.z)
            );

            return boxCollider.transform.TransformPoint(point);
        }

        public static Vector3 GetValidNavMeshPosition (Vector3 position)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(position, out hit, 100, -1))
            {
                return hit.position;
            }
            return position;
        }
    }
}
