using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    public Canvas[] canvases; // 存储所有 Canvas
    public Transform referenceCanvasTransform; // 参考 Canvas 的 Transform

    void Start()
    {
        foreach (var canvas in canvases)
        {
            canvas.gameObject.SetActive(false); // 默认隐藏所有 Canvas
        }
    }

    public void ShowCanvas(int index)
    {
        // 检查索引是否有效
        if (index < 0 || index >= canvases.Length)
        {
            Debug.LogWarning("Invalid canvas index: " + index);
            return;
        }

        // 遍历所有 Canvas，激活选中的，隐藏其他的
        for (int i = 0; i < canvases.Length; i++)
        {
            canvases[i].gameObject.SetActive(i == index);

            if (i == index)
            {
                // 基于参考 Canvas 的位置和旋转设置目标 Canvas
                canvases[i].transform.position = referenceCanvasTransform.position ;
                canvases[i].transform.rotation = referenceCanvasTransform.rotation;

                Debug.Log($"Canvas {index} is now active at the adjusted position relative to the reference canvas.");
            }
        }


                



    }
}





// using UnityEngine;

// public class CanvasManager : MonoBehaviour
// {
//     public Canvas[] canvases; // 存储所有 Canvas
//     public Transform centerEyeAnchor; // CenterEyeAnchor 的 Transform



//     void Start()
//     {
//         foreach (var canvas in canvases)
//         {
//             canvas.gameObject.SetActive(false); // 默认隐藏所有 Canvas
//         }
//     }







//     public void ShowCanvas(int index)
//     {
//         // 检查索引是否有效
//         if (index < 0 || index >= canvases.Length)
//         {
//             Debug.LogWarning("Invalid canvas index: " + index);
//             return;
//         }

//         // 遍历所有 Canvas，激活选中的，隐藏其他的
//         for (int i = 0; i < canvases.Length; i++)
//         {
//             canvases[i].gameObject.SetActive(i == index);

//             if (i == index)
//             {
//                 // 将 Canvas 移动到 CenterEyeAnchor 的位置和方向
//                 canvases[i].transform.position = centerEyeAnchor.position + centerEyeAnchor.forward * 0.5f; // 距离 CenterEyeAnchor 0.5 米
//                 canvases[i].transform.rotation = centerEyeAnchor.rotation;

//                 Debug.Log("Canvas " + index + " is now active at CenterEyeAnchor position.");
//             }
//         }
//     }
// }