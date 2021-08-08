#ifndef _ULTRAFACEBARRACUDA_COMMON_H_
#define _ULTRAFACEBARRACUDA_COMMON_H_

// Pre-defined constants from our UltraFace model
#define MAX_DETECTION 512

// Detection data structure - The layout of this structure must be matched
// with the one defined in Detection.cs.
struct Detection
{
    float x1, y1, x2, y2;
    float score;
    float3 pad;
};

// Common math functions

float CalculateIOU(in Detection d1, in Detection d2)
{
    float area0 = (d1.x2 - d1.x1) * (d1.y2 - d1.y1);
    float area1 = (d2.x2 - d2.x1) * (d2.y2 - d2.y1);

    float x1 = max(d1.x1, d2.x1);
    float x2 = min(d1.x2, d2.x2);
    float y1 = max(d1.y1, d2.y1);
    float y2 = min(d1.y2, d2.y2);
    float areaInner = max(0, x2 - x1) * max(0, y2 - y1);

    return areaInner / (area0 + area1 - areaInner);
}

#endif
