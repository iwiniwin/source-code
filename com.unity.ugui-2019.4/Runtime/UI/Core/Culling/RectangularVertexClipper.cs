namespace UnityEngine.UI
{
    internal class RectangularVertexClipper
    {
        readonly Vector3[] m_WorldCorners = new Vector3[4];
        readonly Vector3[] m_CanvasCorners = new Vector3[4];

        public Rect GetCanvasRect(RectTransform t, Canvas c)
        {
            if (c == null)
                return new Rect();

            t.GetWorldCorners(m_WorldCorners);  // 获取RectTransform矩形在世界空间下的四个角坐标 左下开始，旋转到左上， 然后到右上，最后到右下
            var canvasTransform = c.GetComponent<Transform>();
            for (int i = 0; i < 4; ++i)
                m_CanvasCorners[i] = canvasTransform.InverseTransformPoint(m_WorldCorners[i]);  // 将 position 从世界空间变换到本地空间

            return new Rect(m_CanvasCorners[0].x, m_CanvasCorners[0].y, m_CanvasCorners[2].x - m_CanvasCorners[0].x, m_CanvasCorners[2].y - m_CanvasCorners[0].y);
        }
    }
}
