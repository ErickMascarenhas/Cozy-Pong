# ── .latexmkrc — coloque na RAIZ do projeto (ao lado de main.tex) ──
# Motor: pdfLaTeX (a classe JITH usa inputenc/fontenc = pdflatex)
$pdf_mode  = 1;
$pdflatex  = 'pdflatex -interaction=nonstopmode -file-line-error -synctex=1 %O %S';

# Faz o latexmk rodar makeglossaries sozinho (glossario + siglas do seu TCC)
add_cus_dep('acn', 'acr', 0, 'makeglossaries');
add_cus_dep('glo', 'gls', 0, 'makeglossaries');
$clean_ext .= ' acr acn alg glo gls glg ist xdy nav snm run.xml bbl';
sub makeglossaries {
    my ($base, $path) = fileparse($_[0]);
    pushd $path;
    my $ret = system("makeglossaries $base");
    popd;
    return $ret;
}
