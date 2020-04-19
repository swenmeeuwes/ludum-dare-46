using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntManager : MonoBehaviour
{
    [SerializeField] private Vector3Int _foragerStart;
    [SerializeField] private Ant _antPrefab;
    public Vector3Int ForagerStart => _foragerStart;

    private List<Ant> _ants = new List<Ant>();

    public static AntManager Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }

    public void Register(Ant ant) {
        _ants.Add(ant);
    }

    public void Deregister(Ant ant) {
        _ants.Remove(ant);
    }

    public void RequestBuildAt(Vector2Int position, RoomType constructionType) {
        var builders = _ants.FindAll(a => a.Type == Ant.AntType.Builder);

        var builder = builders[Random.Range(0, builders.Count)];
        builder.BuildAt(position, constructionType);
    }

    public void CreateAntAt(Vector3 position, Ant.AntType antType) {
        var ant = Instantiate(_antPrefab);
        ant.Type = antType;
        ant.transform.position = position;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(_foragerStart, Vector3.one);
    }
}
