#ifndef _ULTRAFACEBARRACUDA_COMMON_H_
#define _ULTRAFACEBARRACUDA_COMMON_H_

// Pre-defined constants from our UltraFace model
#define IMAGE_WIDTH 320
#define IMAGE_HEIGHT 240
#define MAX_DETECTION 4420

// Bounding box structure used for storing object detection results
struct BoundingBox
{
    float x1, y1, x2, y2;
    float score;
    float3 pad;
};

// Common math functions

float CalculateIOU(BoundingBox box1, BoundingBox box2)
{
    float area0 = (box1.x2 - box1.x1) * (box1.y2 - box1.y1);
    float area1 = (box2.x2 - box2.x1) * (box2.y2 - box2.y1);

    float x1 = max(box1.x1, box2.x1);
    float x2 = min(box1.x2, box2.x2);
    float y1 = max(box1.y1, box2.y1);
    float y2 = min(box1.y2, box2.y2);
    float areaInner = max(0, x2 - x1) * max(0, y2 - y1);

    return areaInner / (area0 + area1 - areaInner);
}

#endif
