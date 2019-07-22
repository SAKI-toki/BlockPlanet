using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// タイトルのブロック生成
/// </summary>
public class TitleCreation : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField]
    GameObject titleCube;
    [SerializeField]
    GameObject titleStrongCube;

    private TextAsset csvfile;


    //CSVデータの行数
    const int LineN = 52;
    //CSVデータの列数
    const int RowN = 100;


    //設置位置
    Vector3 position = new Vector3();

    void Start()
    {
        GameObject field = new GameObject("TitleFieldtemp");
        Creation(field);
        Renderer[] meshRenderers = field.GetComponentsInChildren<Renderer>();
        Dictionary<string, List<CombineInstance>> instances = new Dictionary<string, List<CombineInstance>>();
        var cmesh = new CombineInstance();
        cmesh.mesh = CreateMesh(titleCube.GetComponent<MeshFilter>().sharedMesh);
        GameObject parentField = new GameObject("TitleField");
        //メッシュ統合は頂点数がushortの最大値(65,535)を超えるとバグるので、
        //ここでは2回に分けて処理する
        for (int i = 0; i < 2; ++i)
        {
            instances.Clear();
            //統合するメッシュをリストに追加
            for (int j = i * meshRenderers.Length / 2; j < (i + 1) * meshRenderers.Length / 2; ++j)
            {
                var tag = meshRenderers[j].tag;
                if (!instances.ContainsKey(tag))
                {
                    instances.Add(tag, new List<CombineInstance>());
                }
                cmesh.transform = meshRenderers[j].transform.localToWorldMatrix;
                instances[tag].Add(cmesh);
            }
            //統合する
            foreach (var instance in instances)
            {
                GameObject obj = new GameObject(instance.Key + i.ToString());
                //必要なコンポーネントを追加
                MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
                MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
                MeshCollider collider = obj.AddComponent<MeshCollider>();
                //マテリアルのセット
                renderer.sharedMaterial = titleCube.GetComponent<MeshRenderer>().sharedMaterial;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                //タグのセット
                obj.tag = instance.Key;
                meshFilter.mesh = new Mesh();
                //統合
                meshFilter.mesh.CombineMeshes(instance.Value.ToArray());
                collider.sharedMesh = meshFilter.mesh;
                obj.isStatic = true;
                obj.transform.parent = parentField.transform;
            }
        }
        field.SetActive(false);
        string path = "Assets/Models/TitleField";
        System.IO.Directory.CreateDirectory(path);
        Dictionary<string, int> tagNum = new Dictionary<string, int>();
        foreach (var filter in parentField.GetComponentsInChildren<MeshFilter>())
        {
            string name = filter.GetComponent<MeshRenderer>().tag;
            if (!tagNum.ContainsKey(name))
            {
                tagNum.Add(name, 1);
            }
            else
            {
                tagNum[name] += 1;
            }
            UnityEditor.AssetDatabase.CreateAsset(filter.mesh, path + "/" + name + tagNum[name] + ".asset");
        }
        UnityEditor.PrefabUtility.SaveAsPrefabAsset(parentField, "Assets/Prefabs/Title/TitleField.prefab");

    }

    void Creation(GameObject parent)
    {
        //文字検索用
        int[] iDat = new int[4];
        //CSVの全文字列を保存する
        string str = "";
        //取り出した文字列を保存する
        string strget = "";
        TextAsset csvfile = Resources.Load("csv/title") as TextAsset;
        //文字列読み取る
        StringReader reader = new StringReader(csvfile.text);

        //戻り値:使用できる文字がないか、ストリームがシークをサポートしていない場合は -1
        while (reader.Peek() > -1)
        {
            string line = reader.ReadLine();
            //真ん中の点が無いと12とか21が出るよ
            str = str + "," + line;
        }

        //最後に検索文字列の","を追記。最後を取りこぼす
        str = str + ",";

        for (int z = 0; z < LineN; z++)
        {
            for (int x = 0; x < RowN; x++)
            {
                //IndexOfメソッドは文字列内に含まれる文字、文字列の位置を取得することができる。
                iDat[0] = str.IndexOf(",", iDat[0]);
                //次の","を検索
                iDat[1] = str.IndexOf(",", iDat[0] + 1);
                //何文字取り出すか決定
                iDat[2] = iDat[1] - iDat[0] - 1;
                //iDat[2]文字ぶんだけ取り出す
                strget = str.Substring(iDat[0] + 1, iDat[2]);
                //文字列を数値型に変換  
                iDat[3] = int.Parse(strget);
                //次のインデックスへ
                iDat[0]++;

                position.Set(x, 0, z);
                GameObject cube;

                //透明
                if (iDat[3] == 0)
                {
                    cube = Instantiate(titleCube, position, Quaternion.identity);
                    cube.transform.parent = parent.transform;
                }
                //一段
                else
                {
                    cube = Instantiate(titleStrongCube, position, Quaternion.identity);
                    cube.transform.parent = parent.transform;
                }
            }
        }
    }

    Mesh CreateMesh(Mesh cubeMesh)
    {
        Mesh optimizeCubeMesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        int[] indices = cubeMesh.GetTriangles(0);
        cubeMesh.GetVertices(vertices);
        cubeMesh.GetUVs(0, uvs);
        //最適化したIndexを格納する配列
        int[] optimizeIndices = new int[24];
        int index = 0;
        for (int i = 0; i < 6; ++i)
        {
            //奥と下の場合は追加しない
            if (!((vertices[i * 4 + 0].y < 0 &&
            vertices[i * 4 + 1].y < 0 &&
            vertices[i * 4 + 2].y < 0 &&
            vertices[i * 4 + 3].y < 0) ||
            (vertices[i * 4 + 0].z > 0 &&
            vertices[i * 4 + 1].z > 0 &&
            vertices[i * 4 + 2].z > 0 &&
            vertices[i * 4 + 3].z > 0)))
            {
                for (int j = 0; j < 6; ++j)
                {
                    optimizeIndices[index++] = indices[i * 6 + j];
                }
            }
        }
        optimizeCubeMesh = new Mesh();
        optimizeCubeMesh.vertices = vertices.ToArray();
        optimizeCubeMesh.uv = uvs.ToArray();
        optimizeCubeMesh.triangles = optimizeIndices;
        optimizeCubeMesh.RecalculateNormals();
        return optimizeCubeMesh;
    }
#endif
}
