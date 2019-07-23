using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace com.MKG.MB_NC
{
    public class FightMechanics : MonoBehaviour
    {
        public HexGrid Grid { get; set; }

        #region SystemImplementation
        /*public static string [,] FightResults =
        {
           { "-", "-", "B1", "B2-1", "B2", "-1/B2", "-1/B3R", "-1/B4R", "-1/B4R", "-1B5R", "B5R", "-1B5R" },
           { "-1/-1", "-", "-1/B1", "B1-1", "B2-1", "B2-1", "B3", "-1/B3", "-1/B4", "-1/B5", "-1/B5", "-1/B5-1" },
           { "-1/-", "-1/-1", "-", "B1", "B2", "B2", "-1/B2", "B3-1", "B3", "B4", "B5", "B5-1" },
           { "A1", "-1/-", "-1/-1", "-1/B1", "B1-1", "B2", "B2-1", "B2", "B3-1", "B3", "B4-1", "B5" },
           { "A1-1", "A1R", "-1/-", "-", "B1", "B1-1", "B2", "B2-1", "B2", "B3-1", "B3", "B4" },
           { "A1-1", "A1-1", "A1", "-1/-1", "-1/B1", "B1", "B2", "B2", "B2-1", "B2-1", "B3-1", "B3" },
           { "A1", "A1", "A1-1", "-1/-", "-", "-1/B1", "B1-1", "B2", "B2", "B2", "B2", "B3-1" },
           { "A2-1", "A2", "A1", "A1", "-1/-1", "-", "B1", "B1-1", "B2", "B2", "B2-1", "B2" },
           { "A2-1", "A2-1", "A1", "A1-1", "-1/-", "-1/-1", "-1/B1", "B1", "B1-1", "B2", "B2", "B2-1" },
           { "A3-1", "A3", "A2-1", "A1", "A1", "-1/-", "-", "-1/B1", "B1", "B1-1", "B2", "B2" },
           { "A4-1R", "A3-1R", "A2-1R", "A1R", "A1-1", "A1", "-1/-1", "-1/-1", "-1/-1", "-1/-1", "-1/B1-1", "-1/B2-1" }
        };*/
        #endregion

        public static string[,] FightResults =
        {
       { "-", "-", "-/1", "-/2", "-/2", "1/2", "1/3", "1/4", "1/5", "1/5", "-/5", "1/5" },
       { "1/1", "-", "1/1", "-/2", "-/2", "-/2", "-/3", "1/3", "1/4", "1/5", "1/5", "1/5" },
       { "1/-", "1/1", "-", "-/1", "-/2", "-/2", "1/2", "-/3", "-/3", "-/4", "-/5", "-/5" },
       { "1/-", "1/-", "1/1", "1/1", "-/1", "-/2", "-/2", "-/2", "-/3", "-/3", "-/4", "-/5" },
       { "1/-", "1/-", "1/-", "-", "-/1", "-/1", "-/2", "-/2", "-/2", "-/3", "-/3", "-/4" },
       { "1/-", "1/-", "1/-", "1/1", "1/1", "-/1", "-/2", "-/2", "-/2", "-/2", "-/3", "-/3" },
       { "1/-", "1/-", "1/-", "1/-", "-", "1/1", "-/1", "-/2", "-/2", "-/2", "-/2", "-/3" },
       { "2/-", "2/-", "1/-", "1/-", "1/1", "-", "-/1", "-/1", "-/2", "-/2", "-/2", "-/2" },
       { "2/-", "2/-", "1/-", "1/-", "1/-", "1/1", "1/1", "-/1", "-/1", "-/2", "-/2", "-/2" },
       { "3/-", "3/-", "2/-", "1/-", "1/-", "1/-", "-", "1/1", "-/1", "-/1", "-/2", "-/2" },
       { "4/-", "3/-", "2/-", "1/-", "1/-", "1/-", "1/1", "1/1", "1/1", "1/1", "1/1", "1/2" }
    };

        public static string[] AttackingRatios = { "1", "1", "1", "1", "2", "3", "4", "5", "6", "7", "8", "9+" };
        public static string[] DefendingRatios = { "4+", "3", "2", "1", "1", "1", "1", "1", "1", "1", "1", "1" };

        public void HandleVisualAspectOfFight(bool IsCenteredOnEnemy, UnitManager processedUnit)
        {
            processedUnit.SetUnitInfoText();
            if (!IsCenteredOnEnemy)
            {
                processedUnit.Animator.Play("Attack" + Random.Range(1, 3).ToString());
                if (processedUnit.ShouldDie) StartCoroutine(AttackingDie(processedUnit));

                foreach (UnitManager enemy in processedUnit.AttackedEnemies)
                {
                    StartCoroutine(TakeDamageAnimationDelay(enemy));
                    enemy.SetUnitInfoText();
                }
            }
            else
            {
                foreach (UnitManager unit in processedUnit.AttackingEnemies)
                {
                    unit.Animator.Play("Attack" + Random.Range(1, 3).ToString());
                    if (unit.ShouldDie) StartCoroutine(AttackingDie(unit));
                    unit.SetUnitInfoText();
                }
                StartCoroutine(TakeDamageAnimationDelay(processedUnit));
            }
            processedUnit.AttackSound.Play();
        }


        public void ResolveFightCenteredOnEnemy(UnitManager processedUnit)
        {
            int attackingPower = 0, defendingPower = 0;
            int attackingArmor, defendingArmor, terrainDiff = processedUnit.CurrentHex.DefenseModificator;
            bool heightDiff = false;
            defendingArmor = processedUnit.Armor;
            AttackFromBackCheck(processedUnit.AttackingEnemies, new List<UnitManager> { processedUnit });
            if (!processedUnit.AttackedFromBack) defendingPower = processedUnit.Power;
            else { defendingPower = processedUnit.TPower; Debug.Log("attacked from behind"); }
            processedUnit.MarkerRenderer.sprite = processedUnit.Markers[1];

            List<int> ArmorValues = new List<int> { 0, 0, 0 };
            foreach (UnitManager unit in processedUnit.AttackingEnemies)
            {
                if (!heightDiff)
                {
                    if (unit.transform.position.y - processedUnit.transform.position.y > 2f)
                    {
                        heightDiff = true;
                        terrainDiff++;
                    }
                }
                ArmorValues[unit.Armor - 1]++;
                attackingPower += unit.Power;
                unit.MarkerRenderer.sprite = unit.Markers[4];
            }
            List<int> armorMax = new List<int> { 0, -1 }; // value, index
            for (int i = 0; i < 3; i++)
            {
                if (ArmorValues[i] > armorMax[0])
                {
                    armorMax[0] = ArmorValues[i];
                    armorMax[1] = i;
                }
                else if (ArmorValues[i] == armorMax[0])
                {
                    if (i > armorMax[1])
                    {
                        armorMax[0] = ArmorValues[i];
                        armorMax[1] = i;
                    }
                }
            }
            attackingArmor = armorMax[1] + 1;

            List<FightResult> results = GetFightResult(attackingArmor - defendingArmor, terrainDiff, attackingPower, defendingPower);

            if (results[0] != null)
            {
                processedUnit.AttackingEnemies.Sort((y, x) => x.Power.CompareTo(y.Power));
                List<UnitManager> AttackingEnemiesCopy = new List<UnitManager>();
                foreach (UnitManager enemy in processedUnit.AttackingEnemies) AttackingEnemiesCopy.Add(enemy);
                while (results[0].Damage > 0)
                {
                    for (int i = 0; i < AttackingEnemiesCopy.Count; i++)
                    {
                        AttackingEnemiesCopy[i].Power--;
                        results[0].Damage--;
                        if (AttackingEnemiesCopy[i].Power == 0)
                        {
                            AttackingEnemiesCopy[i].ShouldDie = true;
                            AttackingEnemiesCopy.Remove(AttackingEnemiesCopy[i]);
                        }
                        else if (AttackingEnemiesCopy[i].Power == 1)
                            AttackingEnemiesCopy[i].SwitchState();
                        if (results[0].Damage == 0) break;
                    }
                    if (AttackingEnemiesCopy.Count == 0) break;
                }

            }
            if (results[1] != null)
            {
                while (results[1].Damage > 0)
                {
                    results[1].Damage--;
                    processedUnit.Power--;
                    if (processedUnit.Power == 0)
                    {
                        processedUnit.ShouldDie = true;
                        break;
                    }
                    else if (processedUnit.Power == 1)
                        processedUnit.SwitchState();
                }
            }
        }

        public void ResolveFightCenteredOnUnit(UnitManager processedUnit)
        {
            int attackingPower = 0, defendingPower = 0;
            int attackingArmor, defendingArmor;
            attackingPower = processedUnit.Power;
            attackingArmor = processedUnit.Armor;
            processedUnit.MarkerRenderer.sprite = processedUnit.Markers[4];
            List<int> ArmorValues = new List<int> { 0, 0, 0 };
            int maxDefenseModificator = 0;
            AttackFromBackCheck(new List<UnitManager> { processedUnit }, processedUnit.AttackedEnemies);
            foreach (UnitManager enemy in processedUnit.AttackedEnemies)
            {
                int currDefenseModificator = enemy.CurrentHex.DefenseModificator;
                if (enemy.transform.position.y - processedUnit.transform.position.y > 2f)
                    currDefenseModificator++;
                if (currDefenseModificator > maxDefenseModificator)
                    maxDefenseModificator = currDefenseModificator;
                ArmorValues[enemy.Armor - 1]++;
                if (!enemy.AttackedFromBack) defendingPower += enemy.Power;
                else
                {
                    defendingPower += enemy.TPower; Debug.Log("attacked from behind");
                }
                enemy.MarkerRenderer.sprite = enemy.Markers[1];
            }
            List<int> armorMax = new List<int> { 0, -1 }; // value, index
            for (int i = 0; i < 3; i++)
            {
                if (ArmorValues[i] > armorMax[0])
                {
                    armorMax[0] = ArmorValues[i];
                    armorMax[1] = i;
                }
                else if (ArmorValues[i] == armorMax[0])
                {
                    if (i > armorMax[1])
                    {
                        armorMax[0] = ArmorValues[i];
                        armorMax[1] = i;
                    }
                }
            }
            defendingArmor = armorMax[1] + 1;
            List<FightResult> results = GetFightResult(attackingArmor - defendingArmor, maxDefenseModificator, attackingPower, defendingPower);

            if (results[0] != null)
            {
                while (results[0].Damage > 0)
                {
                    results[0].Damage--;
                    processedUnit.Power--;
                    if (processedUnit.Power == 0)
                    {
                        processedUnit.ShouldDie = true;
                        break;
                    }
                    else if (processedUnit.Power == 1)
                        processedUnit.SwitchState();
                }
            }
            if (results[1] != null)
            {
                processedUnit.AttackedEnemies.Sort((y, x) => x.Power.CompareTo(y.Power));
                List<UnitManager> AttackedEnemiesCopy = new List<UnitManager>();
                foreach (UnitManager enemy in processedUnit.AttackedEnemies) AttackedEnemiesCopy.Add(enemy);
                while (results[1].Damage > 0)
                {
                    for (int i = 0; i < AttackedEnemiesCopy.Count; i++)
                    {
                        results[1].Damage--;
                        AttackedEnemiesCopy[i].Power--;
                        if (AttackedEnemiesCopy[i].Power == 0)
                        {
                            AttackedEnemiesCopy[i].ShouldDie = true;
                            AttackedEnemiesCopy.Remove(AttackedEnemiesCopy[i]);
                        }
                        else if (AttackedEnemiesCopy[i].Power == 1)
                            AttackedEnemiesCopy[i].SwitchState();
                        if (results[1].Damage == 0) break;
                    }
                    if (AttackedEnemiesCopy.Count == 0) break;
                }
            }
        }


        public void AttackFromBackCheck(List<UnitManager> attacking, List<UnitManager> defending)
        {
            foreach (UnitManager enemy in defending)
            {
                HexDirection oppositeRotation = enemy.CurrentRotation.Opposite();
                foreach (UnitManager unit in attacking)
                {
                    if (unit.CurrentHex == enemy.CurrentHex.GetNeighbor(oppositeRotation.Previous()) ||
                        unit.CurrentHex == enemy.CurrentHex.GetNeighbor(oppositeRotation) ||
                        unit.CurrentHex == enemy.CurrentHex.GetNeighbor(oppositeRotation.Next()))
                    {
                        enemy.AttackedFromBack = true;
                        break;
                    }
                }
            }
        }

        public IEnumerator TakeDamageAnimationDelay(UnitManager unit)
        {
            yield return new WaitForSeconds(0.3f);
            unit.Animator.Play("Take_damage");
            if (unit.ShouldDie)
            {
                if (unit.Side == GameManager.Side.Northman) GameManager.VikingCounter--;
                else GameManager.AngloSaxonCounter--;
                unit.Animator.SetBool("Death" + Random.Range(1, 3).ToString(), true);
                yield return new WaitForSeconds(1.6f);
                unit.transform.GetChild(2).gameObject.SetActive(false);
                unit.transform.position = new Vector3(10000, 10000, 10000);
                yield return new WaitForSeconds(0.1f);
                unit.Deactivate();
            }
        }

        public IEnumerator AttackingDie(UnitManager unit)
        {
            if (unit.Side == GameManager.Side.Northman) GameManager.VikingCounter--;
            else GameManager.AngloSaxonCounter--;
            yield return new WaitForSeconds(0.5f);
            unit.Animator.Play("Death" + Random.Range(1, 3).ToString());
            yield return new WaitForSeconds(1.4f);
            unit.transform.GetChild(2).gameObject.SetActive(false);
            unit.transform.position = new Vector3(10000, 10000, 10000);
            yield return new WaitForSeconds(0.1f);
            unit.Deactivate();
        }


        public List<FightResult> GetFightResult(int protecionLevelDiff, int terrainDiff, int attackingPower, int defendingPower)
        {
            Debug.Log("Pure Attacking Power (no terrain, armor) : " + attackingPower);
            Debug.Log("Pure Defending Power (no terrain, armor) : " + defendingPower);
            Debug.Log("Terrain Diff: " + terrainDiff);
            int ratioInt, index;
            float ratio = (float)attackingPower / defendingPower;
            if (ratio < 1)
            {
                ratioInt = Mathf.RoundToInt(1 / ratio);
                if (ratioInt >= 4) index = 0;
                else if (ratioInt == 3) index = 1;
                else if (ratioInt == 2) index = 2;
                else index = 3;

            }
            else
            {
                ratioInt = Mathf.RoundToInt(ratio);
                if (ratioInt >= 9) index = 11;
                else if (ratioInt == 8) index = 10;
                else if (ratioInt == 7) index = 9;
                else if (ratioInt == 6) index = 8;
                else if (ratioInt == 5) index = 7;
                else if (ratioInt == 4) index = 6;
                else if (ratioInt == 3) index = 5;
                else if (ratioInt == 2) index = 4;
                else index = 3;
            }


            int realIndex = index + protecionLevelDiff - terrainDiff;
            if (realIndex < 0) index = 0;
            else if (realIndex > 11) index = 11;

            Grid.Ratio[0].text = AttackingRatios[realIndex].ToString() + "(" + AttackingRatios[index].ToString() + ")";
            Grid.Ratio[1].text = DefendingRatios[realIndex].ToString() + "(" + DefendingRatios[index].ToString() + ")";

            List<FightResult> fr = FightResult.Parser(FightResults[DrawDices(), realIndex]);
            Grid.Result.text = "-" + (fr[0] != null ? fr[0].Damage.ToString() : "0") + " pwr.                  -" + (fr[1] != null ? fr[1].Damage.ToString() : "0") + " pwr.";
            return fr;
        }

        public int DrawDices()
        {
            int firstDice = Random.Range(0, 6);
            Grid.DicesImages[0].sprite = Grid.DicesSprites[firstDice];
            int secondDice = Random.Range(0, 6);
            Grid.DicesImages[1].sprite = Grid.DicesSprites[secondDice];
            return firstDice + secondDice;
        }
    }
}
