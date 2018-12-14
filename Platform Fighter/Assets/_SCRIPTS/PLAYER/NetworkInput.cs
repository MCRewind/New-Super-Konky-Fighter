using System;
using System.Collections.Generic;
using System.Linq;
using DATA;
using MANAGERS;
using NETWORKING;
using UnityEngine;
using UnityScript.Steps;
using Types = DATA.Types;

namespace PLAYER
{
    public class NetworkInput : InputSender
    {            
        private bool _queueEvaluation;
        private bool _queueParse;
        private bool _predicting;
        
        private int _framesOfPrediction;
        
        private List<P2PInputSet> _receivedInputSets;
        private List<P2PInputSet> _queuedInputSets;
        private List<P2PInputSet> _predictedInputSets;
        
        protected override void Awake()
        {
            base.Awake();
            _receivedInputSets = new List<P2PInputSet>();
            _queuedInputSets = new List<P2PInputSet>();
            _predictedInputSets = new List<P2PInputSet>();
        }

        public void GiveInputs(P2PInputSet recievedInputs)
        {
            _receivedInputSets.Add(recievedInputs);
        }
        
        private void FixedUpdate()
        {
            if (GameManager.Instance.MatchType == Types.MatchType.OnlineMultiplayer && !MatchStateManager.Instance.ReadyToFight)
                return;

            Debug.Log(_receivedInputSets.Count);
            
            HandleInputs();
        }

        private void HandleInputs()
        {
            _queueEvaluation = false;
            _queueParse = false;
            
            if (_receivedInputSets.Count > 0)
            {
                var queuedParseInput = new P2PInputSet();
                foreach (var receivedInputs in _receivedInputSets)
                {
                    var receivedInputFrame = receivedInputs.Frame % 600;
                    
                    var tempFramesLapsed = P2PHandler.Instance.FramesLapsed;
                    if (P2PHandler.Instance.FramesLapsed < 300 && receivedInputs.Frame > 300)
                        tempFramesLapsed += 600;

                    if (receivedInputFrame == tempFramesLapsed)
                    {
                        queuedParseInput = receivedInputs;
                        _queueParse = true;
                    }
                    else if (receivedInputFrame < tempFramesLapsed)
                        _queueEvaluation = true;
                    else if (receivedInputFrame > tempFramesLapsed)
                        _queuedInputSets.Add(receivedInputs);
                }

                if (_queueEvaluation)
                {
                    for (var i = 0; i < _predictedInputSets.Count; i++)
                    {
                        foreach (var receivedInputSet in _receivedInputSets)
                        {
                            if (receivedInputSet.Frame == _predictedInputSets[i].Frame)
                            {
                                if (!_receivedInputSets[i].Inputs.SequenceEqual(_predictedInputSets[i].Inputs))
                                {
                                    RollbackManager.Instance.Rollback(0);
                                    _receivedInputSets.Clear();
                                    _predictedInputSets.Clear();
                                    return;
                                }

                                break;
                            }
                        }
                    }
                }

                if (_queueParse)
                {
                    ParseInputs(queuedParseInput);   
                    _receivedInputSets.Clear();
                    RollbackManager.Instance.SaveGameState();
                }        
            }
            else
            {
                if (_queuedInputSets.Count != 0)
                {
                    for (var i = 0; i < _queuedInputSets.Count; i++)
                    {
                        var queuedInputSet = _queuedInputSets[i];
                        if (queuedInputSet.Frame == P2PHandler.Instance.FramesLapsed)
                        {
                            _receivedInputSets.Add(queuedInputSet);
                            _queuedInputSets.RemoveAt(i);
                            HandleInputs();
                            return;
                        }
                    }
                }
                
                _predictedInputSets.Add(PredictInputs());
                Debug.Log("predicting...");
                return;
            }
        }
        
        private P2PInputSet PredictInputs()
        {
            return new P2PInputSet();            
        }
    
        public void ParseInputs(P2PInputSet inputSet)
        {
            foreach (var inputChange in inputSet.Inputs)
            {
                Inputs[(int) inputChange.InputType] = inputChange.State;
            }
        }

    }
}