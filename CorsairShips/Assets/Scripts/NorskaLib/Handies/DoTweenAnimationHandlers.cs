using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public struct SequenceNode<T>
{
    public T value;
    public float duration;
    public Ease ease;
}

public class DoTweenAnimationHandler
{
    protected Tween animation;
    protected Sequence sequence;

    public System.Action OnCompleteCallback;

    public bool IsRunning => animation != null || sequence != null;

    /// <summary>
    /// Останавливает анимацию и убивает tweener
    /// </summary>
    /// <param name="completeMotion">Нужно ли завершить анимацию?</param>
    public void Stop(bool completeMotion = false)
    {
        animation?.Kill(completeMotion);

        sequence?.Kill(completeMotion);
    }

    public void SetEase(Ease ease)
    {
        animation?.SetEase(ease);
    }

    public void EnableLoop()
    {
        if (sequence == null)
            animation?.SetLoops(-1);
        else
            sequence?.SetLoops(-1);
    }

    public void DisableLoop()
    {
        animation?.SetLoops(1);

        sequence?.SetLoops(1);
    }
}

public class DoTweenMaterialColorizer : DoTweenAnimationHandler
{
    public Material targetMaterial;
    public float defaultAlpha => defaultColor.a;
    public Color defaultColor;

    public DoTweenMaterialColorizer(Material targetMaterial)
    {
        this.targetMaterial = targetMaterial;
        defaultColor = targetMaterial.color;
    }
    private Tween GetTweener(float toAlpha, float duration, Ease ease = Ease.Linear)
    {
        Color targetColor = targetMaterial.color;
        targetColor.a = toAlpha;

        var tweener = DOTween.To(
            () => targetMaterial.color,
            x => targetMaterial.color = x,
            targetColor,
            duration);

        tweener.SetEase(ease);

        return tweener;
    }
    private Tween GetTweener(Color toColor, float duration, Ease ease = Ease.Linear, bool affectAlpha = false)
    {
        Color targetColor = toColor;
        if (!affectAlpha)
            targetColor.a = targetMaterial.color.a;

        var tweener = DOTween.To(
            () => targetMaterial.color,
            x => targetMaterial.color = x,
            targetColor,
            duration);

        tweener.SetEase(ease);

        return tweener;
    }

    public void SetAlpha(float a)
    {
        Color color = targetMaterial.color;
        color.a = a;
        targetMaterial.color = color;
    }
    public void SetColor(Color color, bool affectAlpha = false)
    {
        if (affectAlpha)
            targetMaterial.color = color;
        else
            targetMaterial.color = new Color(color.r, color.g, color.b, targetMaterial.color.a);
    }

    public void Transit(float fromAlpha, float toAlpha, float duration, Ease ease = Ease.Linear)
    {
        Stop();

        SetAlpha(fromAlpha);

        animation = GetTweener(toAlpha, duration, ease);
    }
    public void Transit(Color fromColor, Color toColor, float duration, Ease ease = Ease.Linear, bool affectAlpha = false)
    {
        Stop();

        SetColor(fromColor, affectAlpha);

        animation = GetTweener(toColor, duration, ease, affectAlpha);
    }
    public void Transit(SequenceNode<float>[] alphas)
    {
        Stop();

        sequence = DOTween.Sequence();
        foreach (var a in alphas)
            sequence.Append(GetTweener(a.value, a.duration, a.ease));
    }

    public void Blink(float fromAlpha, float toAlpha, float durationIn, float durationOut)
    {
        Stop();

        SetAlpha(fromAlpha);

        sequence = DOTween.Sequence();
        sequence.Append(GetTweener(toAlpha, durationIn, Ease.Flash));
        sequence.Append(GetTweener(fromAlpha, durationOut, Ease.Flash));
    }
    public void Blink(Color fromColor, Color toColor, float durationIn, float durationOut, Ease ease = Ease.Flash, bool affectAlpha = false)
    {
        Stop();

        SetColor(fromColor, affectAlpha);

        sequence = DOTween.Sequence();
        sequence.Append(GetTweener(toColor, durationIn, ease, affectAlpha));
        sequence.Append(GetTweener(fromColor, durationOut, ease, affectAlpha));
    }
}

public class DoTweenGraphicColorizer : DoTweenAnimationHandler
{
    private const string GrayscalePropertyName = "_EffectAmount";

    private Graphic targetGraphic;
    public Graphic TargetGraphic
    {
        get => targetGraphic;

        set
        {
            targetGraphic = value;
            defaultColor = targetGraphic.color;
        }
    }

    public float defaultAlpha => defaultColor.a;
    public Color defaultColor;

    public DoTweenGraphicColorizer(Graphic targetGraphic)
    {
        TargetGraphic = targetGraphic;
    }
    private Tween GetTweener(float toAlpha, float duration, Ease ease = Ease.Linear)
    {
        Color targetColor = targetGraphic.color;
        targetColor.a = toAlpha;

        var tweener = DOTween.To(
            () => targetGraphic.color,
            x => targetGraphic.color = x,
            targetColor,
            duration);

        tweener.SetEase(ease);

        return tweener;
    }
    private Tween GetTweener(Color toColor, float duration, Ease ease = Ease.Linear, bool affectAlpha = false)
    {
        Color targetColor = toColor;
        if (!affectAlpha)
            targetColor.a = targetGraphic.color.a;

        var tweener = DOTween.To(
            () => targetGraphic.color,
            x => targetGraphic.color = x,
            targetColor,
            duration);

        tweener.SetEase(ease);

        return tweener;
    }
    private Tween GetGrayscaleTweener(float toGrayscale, float duration, Ease ease = Ease.Linear)
    {
        var tweener = DOTween.To(
            () => targetGraphic.material.GetFloat(GrayscalePropertyName),
            g => targetGraphic.material.SetFloat(GrayscalePropertyName, g),
            toGrayscale,
            duration);

        tweener.SetEase(ease);

        return tweener;
    }

    public void SetAlpha(float a)
    {
        Stop();

        Color color = targetGraphic.color;
        color.a = a;
        targetGraphic.color = color;
    }
    public void SetColor(Color color, bool affectAlpha = false)
    {
        if (affectAlpha)
            targetGraphic.color = color;
        else
            targetGraphic.color = new Color(color.r, color.g, color.b, targetGraphic.color.a);
    }

    public void Transit(float toAlpha, float duration, Ease ease = Ease.Linear)
    {
        Stop();

        animation = GetTweener(toAlpha, duration, ease);
    }
    public void Transit(float fromAlpha, float toAlpha, float duration, Ease ease = Ease.Linear)
    {
        SetAlpha(fromAlpha);

        Transit(toAlpha, duration, ease);
    }
    public void Transit(Color fromColor, Color toColor, float duration, Ease ease = Ease.Linear, bool affectAlpha = false)
    {
        Stop();

        SetColor(fromColor, affectAlpha);

        animation = GetTweener(toColor, duration, ease, affectAlpha);
    }
    public void Transit(Color toColor, float duration, Ease ease = Ease.Linear, bool affectAlpha = false)
    {
        Stop();

        animation = GetTweener(toColor, duration, ease, affectAlpha);
    }
    public void Transit(SequenceNode<float>[] alphas)
    {
        Stop();

        sequence = DOTween.Sequence();
        foreach (var a in alphas)
            sequence.Append(GetTweener(a.value, a.duration, a.ease));
    }

    public void Grayscale(float toGrayscale, float duration, Ease ease = Ease.Linear)
    {
        Stop();

        animation = GetGrayscaleTweener(toGrayscale, duration, ease);
    }

    public void Blink(float fromAlpha, float toAlpha, float durationIn, float durationOut)
    {
        Stop();

        SetAlpha(fromAlpha);

        sequence = DOTween.Sequence();
        sequence.Append(GetTweener(toAlpha, durationIn, Ease.Flash));
        sequence.Append(GetTweener(fromAlpha, durationOut, Ease.Flash));
    }
    public void Blink(Color fromColor, Color toColor, float durationIn, float durationOut, Ease ease = Ease.Flash, bool affectAlpha = false)
    {
        Stop();

        SetColor(fromColor, affectAlpha);

        sequence = DOTween.Sequence();
        sequence.Append(GetTweener(toColor, durationIn, ease, affectAlpha));
        sequence.Append(GetTweener(fromColor, durationOut, ease, affectAlpha));
    }
}

public class DoTweenCanvasFader : DoTweenAnimationHandler
{
    public CanvasGroup targetCanvasGroup;
    public float defaultAlpha;

    public DoTweenCanvasFader(CanvasGroup targetCanvasGroup)
    {
        this.targetCanvasGroup = targetCanvasGroup;
        defaultAlpha = targetCanvasGroup.alpha;
    }
    private Tween GetTweener(float toAlpha, float duration, Ease ease = Ease.Linear)
    {
        var tweener = DOTween.To(
            () => targetCanvasGroup.alpha,
            x => targetCanvasGroup.alpha = x,
            toAlpha,
            duration);

        tweener.SetEase(ease);

        return tweener;
    }

    public void SetAlpha(float a)
    {
        targetCanvasGroup.alpha = a;
    }

    public void Transit(float fromAlpha, float toAlpha, float duration, Ease ease = Ease.Linear)
    {
        Stop();

        SetAlpha(fromAlpha);

        animation = GetTweener(toAlpha, duration, ease);
    }
    public void Transit(float toAlpha, float duration, Ease ease = Ease.Linear)
    {
        Stop();

        animation = GetTweener(toAlpha, duration, ease);
    }
    public void Transit(SequenceNode<float>[] alphas)
    {
        Stop();

        sequence = DOTween.Sequence();
        foreach (var a in alphas)
            sequence.Append(GetTweener(a.value, a.duration, a.ease));
    }

    public void Blink(float fromAlpha, float toAlpha, float durationIn, float durationOut)
    {
        Stop();

        SetAlpha(fromAlpha);

        sequence = DOTween.Sequence();
        sequence.Append(GetTweener(toAlpha, durationIn, Ease.Flash));
        sequence.Append(GetTweener(fromAlpha, durationOut, Ease.Flash));
    }
}

public class DoTweenImageFiller : DoTweenAnimationHandler
{
    public Image targetImage;

    public DoTweenImageFiller(Image targetImage)
    {
        this.targetImage = targetImage;
    }
    private Tween GetTweener(float toAmount, float duration, Ease ease = Ease.Linear)
    {
        var tweener = DOTween.To(
            () => targetImage.fillAmount,
            x => targetImage.fillAmount = x,
            toAmount,
            duration);

        tweener.SetEase(ease);

        return tweener;
    }

    public void Fill(float fromAmount, float toAmount, float duration)
    {
        Stop();

        targetImage.fillAmount = fromAmount;

        animation = GetTweener(toAmount, duration);
    }
    public void Fill(float toAmount, float duration)
    {
        Stop();

        animation = GetTweener(toAmount, duration);
    }
}

public class DoTweenTransformer : DoTweenAnimationHandler
{
    public Transform targetTransform;
    public Vector3 originalScale;

    public DoTweenTransformer(Transform targetTransform)
    {
        this.targetTransform = targetTransform;
        originalScale = targetTransform.localScale;
        //originalScale = targetTransform.localScale;
        //originalScale = targetTransform.localScale;
    }

    private Tween GetScaleTweener(Vector3 toSize, float duration, Ease ease = Ease.Linear)
    {
        var tweener = DOTween.To(
            () => targetTransform.localScale,
            x => targetTransform.localScale = x,
            toSize,
            duration);

        tweener.SetEase(ease);

        return tweener;
    }
    private Tween GetScaleTweener(float toScale, float duration, Ease ease = Ease.Linear)
    {
        var toSize = new Vector3(toScale, toScale, toScale);
        return GetScaleTweener(toSize, duration, ease);
    }
    private Tween GetEulerTweener(Vector3 targetEuler, float duration, Ease ease = Ease.Linear)
    {
        var tweener = DOTween.To(
            () => targetTransform.localRotation,
            x => targetTransform.localRotation = x,
            targetEuler,
            duration);

        tweener.SetRelative(false);
        tweener.SetEase(ease);

        return tweener;
    }
    private Tween GetXEulerTweener(float degrees, bool clockwise, float duration, Ease ease = Ease.Linear)
    {
        var targetEuler = targetTransform.localEulerAngles;
        if (clockwise)
            targetEuler.x += degrees;
        else
            targetEuler.x -= degrees;
        var targetQuaternion = Quaternion.Euler(targetEuler);

        //Debug.Log($"TWEENING LOCAL EULER FROM {targetTransform.localEulerAngles} TO {targetEuler}");
        return GetEulerTweener(targetEuler, duration, ease);
    }
    private Tween GetYEulerTweener(float degrees, bool clockwise, float duration, Ease ease = Ease.Linear)
    {
        var targetEuler = targetTransform.localEulerAngles;
        if (clockwise)
            targetEuler.y += degrees;
        else
            targetEuler.y -= degrees;
        var targetQuaternion = Quaternion.Euler(targetEuler);

        //Debug.Log($"TWEENING LOCAL EULER FROM {targetTransform.localEulerAngles} TO {targetEuler}");
        return GetEulerTweener(targetEuler, duration, ease);

    }
    private Tween GetLocalPositionTweener(Vector3 toPosition, float duration, Ease ease = Ease.Linear)
    {
        var tweener = DOTween.To(
            () => targetTransform.localPosition,
            x => targetTransform.localPosition = x,
            toPosition,
            duration);

        tweener.SetEase(ease);

        return tweener;
    }
    private Tween GetWorldPositionTweener(Vector3 toPosition, float duration, Ease ease = Ease.Linear)
    {
        var tweener = DOTween.To(
            () => targetTransform.position,
            x => targetTransform.position = x,
            toPosition,
            duration);

        tweener.SetEase(ease);

        return tweener;
    }

    public void Reset()
    {
        Stop();
        targetTransform.localScale = originalScale;
    }
    public void SetScale(Vector3 size)
    {
        targetTransform.localScale = size;
    }
    public void SetLocalPosition(Vector3 localPosition)
    {
        targetTransform.localPosition = localPosition;
    }
    public void SetScale(float scale)
    {
        var size = new Vector3(scale, scale, scale);
        SetScale(size);
    }

    public void Move(Vector3 toPosition, float duration, Ease ease = Ease.Linear)
    {
        Stop();
        animation = GetWorldPositionTweener(toPosition, duration, ease);
    }
    public void Move(SequenceNode<Vector3>[] positions)
    {
        Stop();

        sequence = DOTween.Sequence();
        foreach (var p in positions)
            sequence.Append(GetWorldPositionTweener(p.value, p.duration, p.ease));
    }

    public void MoveLocal(Vector3 toPosition, float duration, Ease ease = Ease.Linear)
    {
        Stop();
        animation = GetLocalPositionTweener(toPosition, duration, ease);
    }
    public void MoveLocal(float toHeight, float duration, Ease ease = Ease.Linear)
    {
        var toPosition = targetTransform.localPosition;
        toPosition.y = toHeight;

        MoveLocal(toPosition, duration, ease);
    }
    public void MoveLocal(SequenceNode<Vector3>[] positions)
    {
        Stop();

        sequence = DOTween.Sequence();
        foreach (var p in positions)
            sequence.Append(GetLocalPositionTweener(p.value, p.duration, p.ease));
    }
    public void FloatLocalPosition(float formHeight, float toHeight, float duration)
    {
        Stop();

        var fromPosition = targetTransform.localPosition;
        fromPosition.y = formHeight;

        var toPosition = targetTransform.localPosition;
        toPosition.y = toHeight;

        targetTransform.localPosition = fromPosition;

        //animation = GetLocalPositionTweener(toPosition, duration, Ease.InOutCirc);
        sequence = DOTween.Sequence();
        sequence.Append(GetLocalPositionTweener(toPosition, duration / 2, Ease.InOutSine));
        sequence.Append(GetLocalPositionTweener(fromPosition, duration / 2, Ease.InOutSine));
    }

    public void Rotate(Vector3 targetEuler, float duration, Ease ease = Ease.Linear)
    {
        Stop();

        animation = GetEulerTweener(targetEuler, duration, ease);
    }
    public void RotateVertical(float degrees, bool clockwise, float duration, Ease ease = Ease.Linear)
    {
        Stop();

        animation = GetYEulerTweener(degrees, clockwise, duration, ease);
    }
    public void RotateVerticalCircle(bool clockwise, float duration, Ease ease = Ease.Linear)
    {
        RotateVertical(360, clockwise, duration, ease);
    }
    public void RotateWheel(float degrees, bool clockwise, float duration, Ease ease = Ease.Linear)
    {
        Stop();

        animation = GetXEulerTweener(degrees, clockwise, duration, ease);
    }
    public void RotateWheelCircle(bool clockwise, float duration, Ease ease = Ease.Linear)
    {
        RotateVertical(360, clockwise, duration, ease);
    }

    public void Scale(Vector3 fromSize, Vector3 toSize, float duration, Ease ease = Ease.Linear)
    {
        Stop();

        SetScale(fromSize);

        animation = GetScaleTweener(toSize, duration, ease);
    }
    public void Scale(Vector3 toSize, float duration, Ease ease = Ease.Linear)
    {
        Stop();

        animation = GetScaleTweener(toSize, duration, ease);
    }
    public void Scale(float fromScale, float toScale, float duration, Ease ease = Ease.Linear)
    {
        var fromSize = new Vector3(fromScale, fromScale, fromScale);
        var toSize = new Vector3(toScale, toScale, toScale);

        Scale(fromSize, toSize, duration, ease);
    }
    public void Scale(float toScale, float duration, Ease ease = Ease.Linear)
    {
        var toSize = new Vector3(toScale, toScale, toScale);

        Scale(toSize, duration, ease);
    }
    public void ScaleInOut(float fromScale, float toScale, float durationIn, float durationOut)
    {
        Stop();

        SetScale(fromScale);

        sequence = DOTween.Sequence();
        sequence.Append(GetScaleTweener(toScale, durationIn, Ease.Flash));
        sequence.Append(GetScaleTweener(fromScale, durationOut, Ease.Flash));
    }
    public void Scale(SequenceNode<float>[] scales)
    {
        Stop();

        sequence = DOTween.Sequence();
        foreach (var s in scales)
            sequence.Append(GetScaleTweener(s.value, s.duration, s.ease));
    }
}

public class DoTweenRectHandler : DoTweenAnimationHandler
{
    public RectTransform targetTransform;

    public DoTweenRectHandler(RectTransform targetTransform)
    {
        this.targetTransform = targetTransform;
    }

    private Tween GetSizeTweener(Vector2 size, float duration, Ease ease = Ease.Linear)
    {
        var tweener = DOTween.To(
            () => targetTransform.sizeDelta,
            t => targetTransform.sizeDelta = t,
            size,
            duration);

        tweener.SetEase(ease);

        return tweener;
    }
    private Tween GetAnchoredPositionTweener(Vector2 position, float duration, Ease ease = Ease.Linear)
    {
        var tweener = DOTween.To(
            () => targetTransform.anchoredPosition,
            t => targetTransform.anchoredPosition = t,
            position,
            duration);

        tweener.SetEase(ease);

        return tweener;
    }

    public void Size(Vector2 size, float duration, Ease ease = Ease.Linear)
    {
        Stop();

        animation = GetSizeTweener(size, duration, ease);
    }
    public void SetAnchoredPos(Vector2 position)
    {
        targetTransform.anchoredPosition = position;
    }
    public void MoveAnchored(Vector2 position, float duration, Ease ease = Ease.Linear)
    {
        Stop();

        animation = GetAnchoredPositionTweener(position, duration, ease);
    }
    public void MoveAnchored(SequenceNode<Vector2>[] positions)
    {
        Stop();

        sequence = DOTween.Sequence();
        foreach (var p in positions)
            sequence.Append(GetAnchoredPositionTweener(p.value, p.duration, p.ease));
    }
}

public class DoTweenCameraHandler : DoTweenAnimationHandler
{
    public Camera targetCamera;

    public DoTweenCameraHandler(Camera camera)
    {
        targetCamera = camera;
    }

    private Tween GetFOVTweener(float targetFOV, float duration, Ease ease)
    {
        var tweener = DOTween.To(
            () => targetCamera.fieldOfView,
            x => targetCamera.fieldOfView = x,
            targetFOV,
            duration);

        tweener.SetEase(ease);

        return tweener;
    }

    public void TransitFOV(float targetFOV, float duration, Ease ease = Ease.Linear)
    {
        Stop();
        animation = GetFOVTweener(targetFOV, duration, ease);
    }
}
