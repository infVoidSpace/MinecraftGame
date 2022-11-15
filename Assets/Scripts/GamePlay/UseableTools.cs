using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseableTools
{
    public int hand, pickaxe, axe, shovel, hoe ;

    private int MaxUsability = 10;
	public UseableTools(int hand = 0, int pickaxe=0, int axe=0, int shovel=0, int hoe=0)
    {
        
        if (hand > MaxUsability) hand = MaxUsability;
        if (pickaxe > MaxUsability) pickaxe = MaxUsability;
        if (axe > MaxUsability) axe = MaxUsability;
        if (shovel > MaxUsability) shovel = MaxUsability;
        if (hoe > MaxUsability) hoe = MaxUsability;
        this.hand = hand;
        this.pickaxe = pickaxe;
        this.axe = axe;
        this.shovel = shovel;
        this.hoe = hoe;
    }
	
}
