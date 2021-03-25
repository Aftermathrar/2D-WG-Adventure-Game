using System;
using System.Collections;
using System.Collections.Generic;
using ButtonGame.Attributes;
using ButtonGame.Core;
using ButtonGame.Stats.Follower;
using UnityEditor;
using UnityEngine;

namespace ButtonGame.Dialogue
{
    public class SceneTextNode : ScriptableObject
    {
        [SerializeField]
        bool isVariableText = false;
        [SerializeField]
        string text;
        [SerializeField]
        List<string> children = new List<string>();
        [SerializeField]
        Rect rect = new Rect(10, 300, 250, 75);
        [SerializeField]
        TextDescriptionCondition descCondition;

        public string GetEditorText()
        {
            if (isVariableText)
            {
                // implement ienumerable
                return text;
            }
            else
            {
                return text;
            }
        }

        public string GetText(GameObject conversantGO)
        {
            string returnText = text;
            if(isVariableText)
            {
                // Parse text
                returnText = GetDescription(conversantGO);
            }

            returnText = ReplaceSubstringVariables(returnText, conversantGO);

            return returnText;
        }

        private string ReplaceSubstringVariables(string sInput, GameObject conversantGO)
        {
            string sModified = sInput;
            if(sInput.IndexOf("[Name]") >= 0)
            {
                NPCInfo npcInfo = conversantGO.GetComponent<NPCInfo>();
                sModified = sModified.Replace("[Name]", npcInfo.GetCharacterInfo("name"));
            }
            return sModified;
        }

        public string GetDescription(GameObject conversantGO)
        {
            AppearanceStats appearance = conversantGO.GetComponent<AppearanceStats>();
            switch (descCondition.GetPredicate())
            {
                case TextDescriptionPredicate.Height:
                    float height = appearance.GetHeight();
                    return descCondition.GetDescriptionText(height);
                case TextDescriptionPredicate.Weight:
                case TextDescriptionPredicate.FatPercent:
                    float fatPercent = appearance.GetBodyFatPercent();
                    return descCondition.GetDescriptionText(fatPercent);
                case TextDescriptionPredicate.Face:
                    float faceWeight = appearance.GetBodyPartWeight(BodyParts.Face);
                    return descCondition.GetDescriptionText(faceWeight);
                case TextDescriptionPredicate.Neck:
                    float neckWeight = appearance.GetBodyPartWeight(BodyParts.Neck);
                    return descCondition.GetDescriptionText(neckWeight);
                case TextDescriptionPredicate.Arms:
                    float armsWeight = appearance.GetBodyPartWeight(BodyParts.Arms);
                    return descCondition.GetDescriptionText(armsWeight);
                case TextDescriptionPredicate.Breasts:
                    float breastsWeight = appearance.GetBodyPartWeight(BodyParts.Breasts);
                    return descCondition.GetDescriptionText(breastsWeight);
                case TextDescriptionPredicate.Chest:
                    float chestWeight = appearance.GetBodyPartWeight(BodyParts.Chest);
                    return descCondition.GetDescriptionText(chestWeight);
                case TextDescriptionPredicate.Visceral:
                    float visceralWeight = appearance.GetBodyPartWeight(BodyParts.Visceral);
                    return descCondition.GetDescriptionText(visceralWeight);
                case TextDescriptionPredicate.Stomach:
                    float stomachWeight = appearance.GetBodyPartWeight(BodyParts.Stomach);
                    return descCondition.GetDescriptionText(stomachWeight);
                case TextDescriptionPredicate.Waist:
                    float waistWeight = appearance.GetBodyPartWeight(BodyParts.Waist);
                    return descCondition.GetDescriptionText(waistWeight);
                case TextDescriptionPredicate.Hips:
                    float hipsWeight = appearance.GetBodyPartWeight(BodyParts.Hips);
                    return descCondition.GetDescriptionText(hipsWeight);
                case TextDescriptionPredicate.Butt:
                    float buttWeight = appearance.GetBodyPartWeight(BodyParts.Butt);
                    return descCondition.GetDescriptionText(buttWeight);
                case TextDescriptionPredicate.Thighs:
                    float thighsWeight = appearance.GetBodyPartWeight(BodyParts.Thighs);
                    return descCondition.GetDescriptionText(thighsWeight);
                case TextDescriptionPredicate.Calves:
                    float calvesWeight = appearance.GetBodyPartWeight(BodyParts.Calves);
                    return descCondition.GetDescriptionText(calvesWeight);
                case TextDescriptionPredicate.ArmSize:
                    float armSize = appearance.GetBodyPartSize(BodyParts.Arms);
                    return descCondition.GetDescriptionText(armSize);
                case TextDescriptionPredicate.BreastSize:
                    float breastSize = appearance.GetBodyPartSize(BodyParts.Breasts);
                    return descCondition.GetDescriptionText(breastSize);
                case TextDescriptionPredicate.WaistSize:
                    float waistSize = appearance.GetBodyPartSize(BodyParts.Waist);
                    return descCondition.GetDescriptionText(waistSize);
                case TextDescriptionPredicate.HipSize:
                    float hipSize = appearance.GetBodyPartSize(BodyParts.Hips);
                    return descCondition.GetDescriptionText(hipSize);
                case TextDescriptionPredicate.ThighSize:
                    float thighSize = appearance.GetBodyPartSize(BodyParts.Thighs);
                    return descCondition.GetDescriptionText(thighSize);
                case TextDescriptionPredicate.HairColor:
                    return appearance.GetHairColor().ToLower();
                case TextDescriptionPredicate.HairLength:
                    float hairLength = appearance.GetHairLength();
                    return descCondition.GetDescriptionText(hairLength);
                case TextDescriptionPredicate.EyeColor:
                    return appearance.GetEyeColor().ToLower();
                default:
                return "";
            }
        }

        public int GetDescriptionCount()
        {
            return descCondition.GetDescriptionCount();
        }

        public float GetDescriptionLimit(int index)
        {
            return descCondition.GetDescriptionLimit(index);
        }

        public string GetDescriptionText(int index)
        {
            return descCondition.GetDescriptionText(index);
        }

        public Rect GetRect()
        {
            return rect;
        }

        public List<string> GetChildren()
        {
            return children;
        }

        public bool IsVariableText()
        {
            return isVariableText;
        }

        public TextDescriptionPredicate GetDescriptionPredicate()
        {
            return descCondition.GetPredicate();
        }

#if UNITY_EDITOR
        public void SetText(string newText)
        {
            if (newText != text)
            {
                Undo.RecordObject(this, "Update Dialogue Text");
                text = newText;
                EditorUtility.SetDirty(this);
            }
        }

        public void SetDescriptionPredicate(TextDescriptionPredicate predicate)
        {
            Undo.RecordObject(this, "Change Description Predicate");
            descCondition.SetPredicate(predicate);
            EditorUtility.SetDirty(this);
        }

        public void SetNodeHeight(float newHeight)
        {
            rect.height = newHeight;
            EditorUtility.SetDirty(this);
        }

        public void AddChild(string childID)
        {
            Undo.RecordObject(this, "Add Dialogue Link");
            children.Add(childID);
            EditorUtility.SetDirty(this);
        }

        public void RemoveChild(string childID)
        {
            Undo.RecordObject(this, "Remove Dialogue Link");
            children.Remove(childID);
            EditorUtility.SetDirty(this);
        }

        public void SetPosition(Vector2 newPosition)
        {
            Undo.RecordObject(this, "Move Dialogue Node");
            rect.position = newPosition;
            EditorUtility.SetDirty(this);
        }

        public void SetVariableText(bool newIsVariableText)
        {
            Undo.RecordObject(this, "Change Dialogue Speaker");
            isVariableText = newIsVariableText;
            EditorUtility.SetDirty(this);
        }
#endif
    }
}
