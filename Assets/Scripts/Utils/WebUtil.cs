using System;
using System.Collections;
using System.Threading;
using RSG;
using UniRx;
using UnityEngine;

namespace OMDB.Utils
{
    public static class WebUtil
    {
        public static IPromise<string> WebRequest(string url)
        {
            Promise<string> promise = new Promise<string>();

            GetWWW(url).Subscribe(promise.Resolve, promise.Reject);

            return promise;
        }

        public static IPromise<Sprite> ImageRequest(string url)
        {
            Promise<Sprite> promise = new Promise<Sprite>();

            GetImage(url)
                .Subscribe(
                    texture =>
                    {
                        Rect rec = new Rect(0, 0, texture.width, texture.height);
                        Sprite spriteToUse = Sprite.Create(texture, rec, new Vector2(0.5f, 0.5f), 100);
                        promise.Resolve(spriteToUse);

                    },
                    promise.Reject);

            return promise;
        }

        private static IObservable<string> GetWWW(string url)
        {
            // convert coroutine to IObservable
            return Observable.FromCoroutine<string>((observer, cancellationToken) =>
                GetWWWCore(url, observer, cancellationToken));
        }

        // IObserver is a callback publisher
        // Note: IObserver's basic scheme is "OnNext* (OnError | Oncompleted)?" 
        private static IEnumerator GetWWWCore(string url, IObserver<string> observer, CancellationToken cancellationToken)
        {
            var www = new UnityEngine.WWW(url);
            while (!www.isDone && !cancellationToken.IsCancellationRequested)
            {
                yield return null;
            }

            if (cancellationToken.IsCancellationRequested) yield break;

            if (www.error != null)
            {
                observer.OnError(new Exception(www.error));
            }
            else
            {
                observer.OnNext(www.text);
                observer.OnCompleted(); // IObserver needs OnCompleted after OnNext!
            }
        }

        private static IObservable<Texture2D> GetImage(string url)
        {
            // convert coroutine to IObservable
            return Observable.FromCoroutine<Texture2D>((observer, cancellationToken) =>
                GetImageCore(url, observer, cancellationToken));
        }

        // IObserver is a callback publisher
        // Note: IObserver's basic scheme is "OnNext* (OnError | Oncompleted)?" 
        private static IEnumerator GetImageCore(string url, IObserver<Texture2D> observer, CancellationToken cancellationToken)
        {
            var www = new WWW(url);
            // wait until the download is done
            yield return www;
            // Create a texture in DXT1 format
            Texture2D texture = new Texture2D(www.texture.width, www.texture.height, TextureFormat.DXT1, false);
            // assign the downloaded image to sprite
            www.LoadImageIntoTexture(texture);

            while (!www.isDone && !cancellationToken.IsCancellationRequested)
            {
                yield return null;
            }

            if (cancellationToken.IsCancellationRequested) yield break;

            if (www.error != null)
            {
                observer.OnError(new Exception(www.error));
            }
            else
            {
                observer.OnNext(texture);
                observer.OnCompleted(); // IObserver needs OnCompleted after OnNext!
            }
        }
    }
}