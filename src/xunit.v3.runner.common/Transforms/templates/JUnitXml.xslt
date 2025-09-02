<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output cdata-section-elements="failure system-out"/>

  <xsl:template match="/">
    <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="assemblies">
    <testsuites name="Test results">
      <xsl:attribute name="time">
        <xsl:value-of select="sum(assembly/@time)"/>
      </xsl:attribute>
      <xsl:attribute name="tests">
        <xsl:value-of select="sum(assembly/@total)"/>
      </xsl:attribute>
      <xsl:attribute name="failures">
        <xsl:value-of select="sum(assembly/@failed)"/>
      </xsl:attribute>
      <xsl:attribute name="errors">
        <xsl:value-of select="sum(assembly/@errors)"/>
      </xsl:attribute>
      <xsl:attribute name="disabled">
        <xsl:value-of select="sum(assembly/@skipped)"/>
      </xsl:attribute>
      <xsl:apply-templates select="assembly/collection"/>
    </testsuites>
  </xsl:template>

  <xsl:template match="collection">
    <testsuite>
      <xsl:attribute name="name">
        <xsl:value-of select="@name"/>
      </xsl:attribute>
      <xsl:attribute name="time">
        <xsl:value-of select="@time"/>
      </xsl:attribute>
      <xsl:if test="@start-rtf">
        <xsl:attribute name="timestamp">
          <xsl:value-of select="@start-rtf"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:attribute name="tests">
        <xsl:value-of select="@total"/>
      </xsl:attribute>
      <xsl:attribute name="failures">
        <xsl:value-of select="@failed"/>
      </xsl:attribute>
      <xsl:attribute name="skipped">
        <xsl:value-of select="@skipped"/>
      </xsl:attribute>
      <xsl:apply-templates select="test"/>
    </testsuite>
  </xsl:template>

  <xsl:template match="test">
    <testcase>
      <xsl:attribute name="name">
        <xsl:value-of select="@name"/>
      </xsl:attribute>
      <xsl:attribute name="classname">
        <xsl:value-of select="@type"/>
      </xsl:attribute>
      <xsl:if test="@time">
        <xsl:attribute name="time">
          <xsl:value-of select="@time"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="output">
        <system-out>
          <xsl:value-of select="output"/>
        </system-out>
      </xsl:if>
      <xsl:apply-templates select="failure"/>
      <xsl:apply-templates select="reason"/>
    </testcase>
  </xsl:template>

  <xsl:template match="failure">
    <failure>
      <xsl:attribute name="type">
        <xsl:value-of select="@exception-type"/>
      </xsl:attribute>
      <xsl:if test="message">
        <xsl:attribute name="message">
          <xsl:value-of select="message"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="stack-trace">
        <xsl:value-of select="stack-trace" />
      </xsl:if>
    </failure>
  </xsl:template>

  <xsl:template match="reason">
    <skipped>
      <xsl:attribute name="message">
        <xsl:value-of select="."/>
      </xsl:attribute>
    </skipped>
  </xsl:template>

</xsl:stylesheet>
