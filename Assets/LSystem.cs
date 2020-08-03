/* File : LSystem.cs
 * Author : Derek Nguyen
 * Description: Creates a procedurally generated tree using LSystem
 *              https://en.wikipedia.org/wiki/L-system
 *              F : Forward, X : Control Curve, + : turn right, - : turn left, [ : push stack, ] : pop stack               
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class LSystem : MonoBehaviour
{
    //editable changes
    public int iterations;
    public float angle;
    public float width;
    public float minPetalLength;
    public float maxPetalLength;
    public float minBranchLength;
    public float maxBranchLength;
    public float variance;

    // what to build the tree on
    public GameObject tree;

    // branch and flower objects that can be instantiated
    public GameObject branch;
    public GameObject blossom;

    private const string AXIOM = "X";

    // the scale of how each thing shrinks after each creation
    private float lengthScale = 0.75f;
    private float widthScale = 0.95f; 

    private Dictionary<char, string> rules = new Dictionary<char, string>();
    private Stack<SavedTransform> savedTransforms = new Stack<SavedTransform>();
    private Vector3 initialPosition;

    private string currentPath = "";
    private float[] randomRotations;

    private void Awake()
    {
        //create a bunch of random variations
        randomRotations = new float[1000];
        for(int i = 0; i < randomRotations.Length; i++)
        {
            randomRotations[i] = Random.Range(-1f, 1f);
        }

        //the rules for each iteration
        rules.Add('X', "F[-FX][/FX][+FX][*FX]");
        rules.Add('F', "FF");
        rules.Add('*', "F*[[X]+X]+F[/FX]-X");
        rules.Add('/', "F/[[X]-X]-F[*FX]+X");


        Generate();
        Debug.Log(currentPath);

        Build();
    }

    private void Generate()
    {
        currentPath = AXIOM;

        StringBuilder stringBuilder = new StringBuilder();

        for(int i = 0; i < iterations; i++)
        {
            char[] currentPathChars = currentPath.ToCharArray();
            for(int j = 0; j < currentPathChars.Length; j++)
            {
                //take a look at this in a bit
                stringBuilder.Append(rules.ContainsKey(currentPathChars[j]) ? rules[currentPath[j]] : currentPathChars[j].ToString());
            }
            currentPath = stringBuilder.ToString();
            stringBuilder = new StringBuilder();
        }
    }

    private void Build()
    {
        for(int i = 0; i < currentPath.Length; i++)
        {
            switch (currentPath[i])
            {
                case 'F':
                    {
                        initialPosition = transform.position;
                        bool isLeaf = false;

                        //sets current object as branch or blossom
                        GameObject currentObject;
                        if(currentPath[(i+ 1) % currentPath.Length] == 'X' || currentPath[(i + 3) % currentPath.Length]  == 'F' && currentPath[(i + 4) % currentPath.Length] == 'X')
                        {
                            currentObject = Instantiate(blossom);
                            isLeaf = true;
                        }
                        else
                        {
                            currentObject = Instantiate(branch);
                        }

                        //set parent
                        currentObject.transform.SetParent(tree.transform);

                        //set position and rotation
                        currentObject.transform.SetPositionAndRotation(transform.position, transform.rotation);
                        //set shape
                        float length;

                        if (isLeaf)
                        {
                            length = Random.Range(minPetalLength, maxPetalLength) * 0.1f;
                            currentObject.transform.localScale = new Vector3(length, length, length);
                        }
                        else
                        {
                            length = Random.Range(minBranchLength, maxBranchLength);
                            currentObject.transform.localScale = new Vector3(width, length, width);
                            transform.Translate(Vector3.up * length * 2f);
                        }
                    }
                    break;

                case 'X':
                    break;

                case '-':
                    transform.Rotate(Vector3.forward * angle * (1f + variance / 100f * randomRotations[i % randomRotations.Length]));
                    break;

                case '+':
                    transform.Rotate(Vector3.back * angle * (1f + variance / 100f * randomRotations[i % randomRotations.Length]));
                    break;

                case '*':
                    transform.Rotate(Vector3.left * angle * (1f + variance / 100f * randomRotations[i % randomRotations.Length]));
                    break;

                case '/':
                    transform.Rotate(Vector3.right * angle * (1f + variance / 100f * randomRotations[i % randomRotations.Length]));
                    break;

                case '[':
                    savedTransforms.Push(new SavedTransform {   position = transform.position,
                                                                rotation = transform.rotation});
                    width *= widthScale;
                    minBranchLength *= lengthScale;
                    maxBranchLength *= lengthScale;
                    break;

                case ']':
                    SavedTransform saved = savedTransforms.Pop();
                    width /= widthScale;
                    minBranchLength /= lengthScale;
                    maxBranchLength /= lengthScale;
                    transform.position = saved.position;
                    transform.rotation = saved.rotation;
                    break;

                default:
                    Debug.LogError("Not a Valid Key: " + currentPath[i]);
                    break;
            }

        }
    }
}
