using UnityEditor;
using UnityEngine;

namespace Assets.McCoy.UI
{
  public class McCoyCityZonePlacementNode : MonoBehaviour
  {
    [SerializeField]
    string _NodeId;
    public string NodeId { get => _NodeId; set => _NodeId = value; }
  }
}