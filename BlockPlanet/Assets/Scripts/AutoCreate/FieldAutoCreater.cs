using UnityEngine;

public class FieldAutoCreater : MonoBehaviour
{
    [SerializeField]
    Transform parentTransform;
    BlockMap blockMap = new BlockMap();
    void Start()
    {
        FieldAutoCreateImpl.AutoCreate();
        BlockCreater.GetInstance().AutoGenerateBlock(FieldAutoCreateImpl.map, parentTransform, blockMap);
        blockMap.BlockRendererUpdate();
    }
}