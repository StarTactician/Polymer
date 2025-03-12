using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barriers : Ball
{
    [SerializeField]
    private bool CanReactWithPositive;
    [SerializeField]
    private bool CanReactWithNegative;
    [SerializeField]
    private bool CanReactWithNeutral;
    [SerializeField]
    private bool CanMove;
}
