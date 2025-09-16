using UnityEngine;

public class QuadTerrainAlign : MonoBehaviour
{
    [SerializeField] private Transform quadTransform; // Assign your Quad transform in the inspector
    [SerializeField] private LayerMask terrainLayer;  // Set this to the layer your terrain is on for accurate raycasting

    private void Start()
    {
        AlignQuadToTerrain();
    }

    private void AlignQuadToTerrain()
    {
        // Get the vertices of the quad
        Mesh quadMesh = quadTransform.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = quadMesh.vertices;

        // Adjust each vertex position based on terrain height
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldVertex = quadTransform.TransformPoint(vertices[i]); // Convert local vertex position to world position
            if (Physics.Raycast(worldVertex + Vector3.up * 100f, Vector3.down, out RaycastHit hit, Mathf.Infinity, terrainLayer))
            {
                // If hit, adjust the vertex position to the hit point
                vertices[i] = quadTransform.InverseTransformPoint(hit.point + Vector3.up * 0.1f); // Convert back to local position
            }
        }

        // Apply the adjusted vertices back to the quad mesh
        quadMesh.vertices = vertices;
        quadMesh.RecalculateBounds();
        quadMesh.RecalculateNormals();
    }
}