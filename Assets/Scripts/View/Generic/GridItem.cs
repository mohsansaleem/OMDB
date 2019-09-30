using System;
using DG.Tweening;
using OMDB.Model;
using OMDB.View;
using RSG;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class GridItem : MonoBehaviour
{
    [Inject] private GridItem.Pool _gridItemPool;
    [Inject] private SignalBus _signalBus;
    
    public RectTransform ThisRectTransform;
    public Image MoviePosterSprite;
    public Button Button;

    private MovieDataModel _model;

    public IPromise DespawnTween()
    {
        Promise p = new Promise();
        var tween = ThisRectTransform.DOSizeDelta(Vector2.zero, 0.5f, true);
        tween.onComplete = p.Resolve;
        tween.Play();

        return p;
    }

    public IPromise SpawnTween()
    {
        Promise p = new Promise();
        ThisRectTransform.sizeDelta = Vector2.zero;
        var tween = ThisRectTransform.DOSizeDelta(new Vector2(100,100), 0.8f, true);
        tween.onComplete = p.Resolve;
        tween.Play();

        return p;
    }
    
    public IPromise MoveToPositionTween(Vector2 pos)
    {
        Promise p = new Promise();
        try
        {
            if (Math.Abs(pos.x - ThisRectTransform.anchoredPosition.x) < 1 && 
                Math.Abs(pos.y - ThisRectTransform.anchoredPosition.y) < 1)
            {
                Promise.Resolved();
            }
            else
            {
                GridItem dummy = null;
                Vector2 dummyTarget;
            
                if (Math.Abs(pos.y - ThisRectTransform.anchoredPosition.y) > 1)
                {
                    dummy = _gridItemPool.Spawn(_model, ThisRectTransform.anchoredPosition, false);

                    if (pos.y > ThisRectTransform.anchoredPosition.y)
                    {
                        dummyTarget = new Vector2(-50, ThisRectTransform.anchoredPosition.y);
                        ThisRectTransform.anchoredPosition = new Vector2(pos.x + 100, pos.y);
                    }
                    else
                    {
                        dummyTarget = new Vector2(ThisRectTransform.anchoredPosition.x + 100, ThisRectTransform.anchoredPosition.y);
                        ThisRectTransform.anchoredPosition = new Vector2(-50, pos.y);
                    }
                
                    dummy.MoveToPositionTween(dummyTarget)
                        .Done((() => _gridItemPool.Despawn(dummy)));
                    
                    
                }
            
                var tween = ThisRectTransform.DOAnchorPos(pos, 0.5f, true);
                tween.onComplete = p.Resolve;
                tween.Play();
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        
        return p;
    }
    
    public class Pool : MonoMemoryPool<MovieDataModel, Vector2, bool, GridItem>
    {
//        protected override void OnSpawned(GridItem item)
//        {
//            //base.OnSpawned(item);
//        }

        protected override void Reinitialize(MovieDataModel model, Vector2 pos, bool withEffect, GridItem gridItem)
        {
            gridItem._model = model;
            
            gridItem.MoviePosterSprite.sprite = model.Poster.Value;
            gridItem.ThisRectTransform.anchoredPosition = pos;
            
            if (withEffect)
            {
                gridItem.ThisRectTransform.sizeDelta = new Vector2(0,0);
                gridItem.SpawnTween();
            }
            else
            {
                gridItem.ThisRectTransform.sizeDelta = new Vector2(100,100);
            }

            // Lame. Refactor.
            gridItem.Button.onClick.RemoveAllListeners();
            gridItem.Button.onClick.AddListener((() => gridItem._signalBus.Fire(new MovieSelectSignal(model))));
            
            //base.OnSpawned(gridItem);
        }
    }
}
