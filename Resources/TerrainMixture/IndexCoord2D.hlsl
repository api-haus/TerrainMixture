#ifndef TM_INDEX_COORD_2D_INC
#define TM_INDEX_COORD_2D_INC

int ToIndex(int2 coord, int side)
{
    return coord.y * side + coord.x;
}

int2 ToCoord(int index, int side)
{
    return int2(
        index % side,
        index / side
    );
}

#endif
