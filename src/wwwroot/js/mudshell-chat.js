// MudShell chat helpers: composer keyboard handling / auto-grow and transcript auto-follow.

export function initComposer(textarea, dotnet) {
    const resize = () => {
        textarea.style.height = 'auto';
        textarea.style.height = `${textarea.scrollHeight}px`;
    };

    const onInput = () => resize();

    const onKeyDown = (e) => {
        if (e.key !== 'Enter' || e.isComposing || e.keyCode === 229) return;
        const modifier = e.ctrlKey || e.metaKey;
        const sendOnEnter = textarea.dataset.sendOnEnter === 'true';
        const shouldSend = sendOnEnter ? (!e.shiftKey && !e.altKey) : modifier;
        if (!shouldSend) return;
        e.preventDefault();
        dotnet.invokeMethodAsync('HandleEnter', textarea.value);
    };

    textarea.addEventListener('input', onInput);
    textarea.addEventListener('keydown', onKeyDown);
    resize();

    return {
        resize,
        setValue: (v) => { textarea.value = v; resize(); },
        focus: () => textarea.focus(),
        dispose: () => {
            textarea.removeEventListener('input', onInput);
            textarea.removeEventListener('keydown', onKeyDown);
        }
    };
}

export function observeTranscript(root, thresholdPx) {
    const scroller = root.querySelector('.mds-chat-transcript-scroll');
    const content = root.querySelector('.mds-chat-transcript-content');
    const jump = root.querySelector('.mds-chat-transcript-jump');
    let stuck = true;

    const atBottom = () => scroller.scrollHeight - scroller.scrollTop - scroller.clientHeight <= thresholdPx;
    const toEnd = (smooth) => scroller.scrollTo({ top: scroller.scrollHeight, behavior: smooth ? 'smooth' : 'auto' });
    const sync = () => root.classList.toggle('mds-chat-transcript--detached', !stuck);

    const onScroll = () => { stuck = atBottom(); sync(); };
    const onJump = () => { stuck = true; sync(); toEnd(true); };

    // Content grows without the transcript itself re-rendering (streaming), so follow its size.
    const ro = new ResizeObserver(() => { if (stuck) toEnd(false); });
    ro.observe(content);
    scroller.addEventListener('scroll', onScroll, { passive: true });
    jump?.addEventListener('click', onJump);
    toEnd(false);

    return {
        scrollToEnd: () => { stuck = true; sync(); toEnd(false); },
        dispose: () => {
            ro.disconnect();
            scroller.removeEventListener('scroll', onScroll);
            jump?.removeEventListener('click', onJump);
        }
    };
}
