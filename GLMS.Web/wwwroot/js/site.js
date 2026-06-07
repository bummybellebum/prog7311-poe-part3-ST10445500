document.addEventListener("DOMContentLoaded", () => {
  const toggles = document.querySelectorAll("[data-mobile-nav-toggle]");
  const closers = document.querySelectorAll("[data-mobile-nav-close]");
  const menu = document.getElementById("mobileMenu");

  if (!menu || toggles.length === 0) {
    return;
  }

  const setOpen = (isOpen) => {
    document.body.classList.toggle("mobile-menu-open", isOpen);
    menu.setAttribute("aria-hidden", String(!isOpen));
    toggles.forEach((toggle) => {
      toggle.setAttribute("aria-expanded", String(isOpen));
    });
  };

  toggles.forEach((toggle) => {
    toggle.addEventListener("click", () => {
      setOpen(!document.body.classList.contains("mobile-menu-open"));
    });
  });

  closers.forEach((closer) => {
    closer.addEventListener("click", () => setOpen(false));
  });

  document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") {
      setOpen(false);
    }
  });
});
