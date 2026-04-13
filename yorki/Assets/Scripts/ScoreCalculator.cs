using UnityEngine;

public static class ScoreCalculator
{
    private const int GRID_SIZE = 8;
    private const int SLIDE_RANGE = 2;
    private const float INK_THRESHOLD = 0.1f; // 배경(흰색)과 다른 정도

    // 플레이어 텍스처와 손님 데이터로 0~100 점수 반환
    public static int Calculate(Texture2D playerTex, CustomerData customer)
    {
        if (playerTex == null || customer == null || customer.referenceImage == null)
            return 0;

        int[] playerGrid = BuildGrid(playerTex);
        int[] refGrid    = BuildGrid(customer.referenceImage);

        float bestScore = 0f;

        // 슬라이딩 윈도우: ±SLIDE_RANGE 범위로 플레이어 그리드 이동
        for (int dy = -SLIDE_RANGE; dy <= SLIDE_RANGE; dy++)
        {
            for (int dx = -SLIDE_RANGE; dx <= SLIDE_RANGE; dx++)
            {
                float score = CompareGrids(playerGrid, refGrid, dx, dy, customer.zoneWeights);
                if (score > bestScore)
                    bestScore = score;
            }
        }

        return Mathf.RoundToInt(Mathf.Clamp(bestScore, 0f, 100f));
    }

    // Texture2D → 8x8 이진 그리드 (잉크 있음=1, 없음=0)
    static int[] BuildGrid(Texture2D tex)
    {
        int[] grid = new int[GRID_SIZE * GRID_SIZE];

        // 읽기 가능한 텍스처로 복사
        RenderTexture rt = RenderTexture.GetTemporary(512, 512, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(tex, rt);
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D readable = new Texture2D(512, 512, TextureFormat.RGBA32, false);
        readable.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
        readable.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);

        int cellSize = 512 / GRID_SIZE; // 64픽셀

        for (int gy = 0; gy < GRID_SIZE; gy++)
        {
            for (int gx = 0; gx < GRID_SIZE; gx++)
            {
                int inkCount = 0;
                int totalPx  = cellSize * cellSize;

                for (int py = 0; py < cellSize; py++)
                {
                    for (int px = 0; px < cellSize; px++)
                    {
                        int tx = gx * cellSize + px;
                        int ty = gy * cellSize + py;
                        Color c = readable.GetPixel(tx, ty);
                        // 흰 배경과의 차이로 잉크 여부 판단
                        float diff = (Mathf.Abs(c.r - 1f) + Mathf.Abs(c.g - 1f) + Mathf.Abs(c.b - 1f)) / 3f;
                        if (diff > INK_THRESHOLD)
                            inkCount++;
                    }
                }

                // 칸의 5% 이상 잉크가 있으면 1
                grid[gy * GRID_SIZE + gx] = (inkCount / (float)totalPx > 0.05f) ? 1 : 0;
            }
        }

        Object.Destroy(readable);
        return grid;
    }

    // 두 그리드 비교 (dx, dy 오프셋 적용)
    static float CompareGrids(int[] playerGrid, int[] refGrid, int dx, int dy, float[] weights)
    {
        float correctScore  = 0f;
        float wrongPenalty  = 0f;
        float totalRefWeight = 0f;
        float totalPlayerInk = 0f;

        for (int gy = 0; gy < GRID_SIZE; gy++)
        {
            for (int gx = 0; gx < GRID_SIZE; gx++)
            {
                int refIdx = gy * GRID_SIZE + gx;
                int refVal = refGrid[refIdx];

                // 플레이어 그리드에서 오프셋 적용한 위치
                int px = gx - dx;
                int py = gy - dy;
                int playerVal = 0;
                if (px >= 0 && px < GRID_SIZE && py >= 0 && py < GRID_SIZE)
                    playerVal = playerGrid[py * GRID_SIZE + px];

                float w = (weights != null && weights.Length == 64) ? weights[refIdx] : 1.0f;

                if (refVal == 1) totalRefWeight += w;
                if (playerVal == 1) totalPlayerInk += w;

                if (refVal == 1 && playerVal == 1) correctScore  += w;
                if (refVal == 0 && playerVal == 1) wrongPenalty  += w * 0.5f;
            }
        }

        if (totalRefWeight == 0f) return 0f;

        float rawScore     = (correctScore / totalRefWeight) * 100f;
        float penaltyScore = (totalPlayerInk > 0f)
            ? (wrongPenalty / totalPlayerInk) * 30f
            : 0f;

        return rawScore - penaltyScore;
    }
}
